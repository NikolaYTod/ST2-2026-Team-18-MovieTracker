
using System.Globalization;
using Microsoft.Data.Sqlite;
using MovieTracker.Models;

namespace MovieTracker.Services;

/// <summary>
/// Резултат от преобразуването на JSON заявка към SQL.
/// </summary>
public sealed record TranslatedSqlQuery(
    string Sql,
    IReadOnlyList<SqliteParameter> Parameters);

/// <summary>
/// Преобразува JSONQuery в безопасна параметризирана
/// SQL SELECT заявка за SQLite.
/// </summary>
public sealed class JsonQuerySqlTranslator
{
    // Позволени колони в таблицата Movies.
    // Имената се определят от приложението, а не от Gemini.
    private static readonly Dictionary<string, string> AllowedFields =
        new(StringComparer.Ordinal)
        {
            ["Title"] = "\"Title\"",
            ["Genre"] = "\"Genre\"",
            ["ReleaseYear"] = "\"ReleaseYear\"",
            ["Rating"] = "\"Rating\"",
            ["IsWatched"] = "\"IsWatched\""
        };

    private static readonly HashSet<string> AllowedOperators =
        new(StringComparer.Ordinal)
        {
            "equals",
            "contains",
            "gt",
            "gte",
            "lt",
            "lte"
        };

    /// <summary>
    /// Преобразува JSON заявка към параметризирана SQL заявка.
    /// </summary>
    public TranslatedSqlQuery Translate(JSONQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Conditions is null || query.Conditions.Count == 0)
            throw new ArgumentException(
                "Заявката трябва да съдържа поне едно условие.");

        if (query.Conditions.Count > 10)
            throw new ArgumentException(
                "Допустими са максимум 10 условия.");

        if (query.LogicalOperator is not ("AND" or "OR"))
            throw new ArgumentException(
                "Невалиден логически оператор.");

        if (query.SortDirection is not ("ASC" or "DESC"))
            throw new ArgumentException(
                "Невалидна посока на сортиране.");

        var parameters = new List<SqliteParameter>();
        var conditions = new List<string>();

        foreach (var condition in query.Conditions)
        {
            if (condition is null ||
                string.IsNullOrWhiteSpace(condition.Field) ||
                string.IsNullOrWhiteSpace(condition.Operator) ||
                condition.Value is null)
            {
                throw new ArgumentException(
                    "Невалидно условие за филтриране.");
            }

            if (!AllowedFields.TryGetValue(
                    condition.Field, out var column))
            {
                throw new ArgumentException(
                    $"Полето '{condition.Field}' не е разрешено.");
            }

            if (!AllowedOperators.Contains(condition.Operator))
            {
                throw new ArgumentException(
                    $"Операторът '{condition.Operator}' не е разрешен.");
            }

            var parameterName = $"$p{parameters.Count}";

            var sqlCondition = BuildCondition(
                condition,
                column,
                parameterName,
                parameters);

            conditions.Add(sqlCondition);
        }

        // Определяме логическата връзка между условията.
        var logicalOperator = query.LogicalOperator;

        var whereClause = string.Join(
            $" {logicalOperator} ",
            conditions);

        // Проверяваме колоната за сортиране.
        var orderBy = "\"Title\"";

        if (!string.IsNullOrWhiteSpace(query.OrderBy))
        {
            if (!AllowedFields.TryGetValue(
                    query.OrderBy, out orderBy))
            {
                throw new ArgumentException(
                    "Невалидно поле за сортиране.");
            }
        }

        var sql = $"""
            SELECT *
            FROM "Movies"
            WHERE {whereClause}
            ORDER BY {orderBy} {query.SortDirection}
            """;

        // Ограничението се добавя само след проверка.
        if (query.Limit.HasValue)
        {
            if (query.Limit.Value < 1 ||
                query.Limit.Value > 100)
            {
                throw new ArgumentException(
                    "Ограничението трябва да бъде между 1 и 100.");
            }

            sql += $" LIMIT {query.Limit.Value}";
        }

        return new TranslatedSqlQuery(sql, parameters);
    }

    /// <summary>
    /// Изгражда отделно SQL условие и добавя параметъра му.
    /// </summary>
    private static string BuildCondition(
        QueryCondition condition,
        string column,
        string parameterName,
        List<SqliteParameter> parameters)
    {
        var field = condition.Field;
        var operation = condition.Operator;
        var value = condition.Value;

        object parameterValue;
        string sqlOperator;

        switch (field)
        {
            case "Title":
            case "Genre":

                if (operation == "equals")
                {
                    sqlOperator = "=";
                    parameterValue = value;
                }
                else if (operation == "contains")
                {
                    sqlOperator = "LIKE";

                    // Екранираме специалните LIKE символи.
                    var escapedValue = value
                        .Replace("\\", "\\\\")
                        .Replace("%", "\\%")
                        .Replace("_", "\\_");

                    parameterValue = $"%{escapedValue}%";
                }
                else
                {
                    throw new ArgumentException(
                        $"Операторът '{operation}' не е подходящ за {field}.");
                }

                break;

            case "ReleaseYear":

                if (!int.TryParse(
                        value,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var year))
                {
                    throw new ArgumentException(
                        "Годината трябва да бъде цяло число.");
                }

                sqlOperator = GetSqlOperator(operation);
                parameterValue = year;
                break;

            case "Rating":

                if (!double.TryParse(
                        value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var rating) ||
                    double.IsNaN(rating) ||
                    double.IsInfinity(rating) ||
                    rating < 0 ||
                    rating > 10)
                {
                    throw new ArgumentException(
                        "Рейтингът трябва да бъде число между 0 и 10.");
                }

                sqlOperator = GetSqlOperator(operation);
                parameterValue = rating;
                break;

            case "IsWatched":

                if (operation != "equals" ||
                    !bool.TryParse(value, out var isWatched))
                {
                    throw new ArgumentException(
                        "IsWatched приема само true или false с equals.");
                }

                sqlOperator = "=";
                parameterValue = isWatched;
                break;

            default:
                throw new ArgumentException(
                    "Неразрешено поле.");
        }

        var parameter = new SqliteParameter(
            parameterName,
            parameterValue);

        parameters.Add(parameter);

        if (operation == "contains")
        {
            return $"{column} {sqlOperator} {parameterName} ESCAPE '\\'";
        }

        return $"{column} {sqlOperator} {parameterName}";
    }

    /// <summary>
    /// Преобразува позволените оператори към SQL оператори.
    /// </summary>
    private static string GetSqlOperator(string operation)
    {
        return operation switch
        {
            "equals" => "=",
            "gt" => ">",
            "gte" => ">=",
            "lt" => "<",
            "lte" => "<=",

            _ => throw new ArgumentException(
                "Неразрешен оператор.")
        };
    }
}

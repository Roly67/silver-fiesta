// <copyright file="BusinessRuleException.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

namespace FileConversionApi.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    /// <param name="ruleCode">The rule code.</param>
    /// <param name="message">The exception message.</param>
    public BusinessRuleException(string ruleCode, string message)
        : base(message)
    {
        this.RuleCode = ruleCode;
    }

    /// <summary>
    /// Gets the rule code.
    /// </summary>
    public string RuleCode { get; }
}

﻿namespace BalancerKube.Common.Models;

public readonly record struct Money : IComparable<Money>
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    public Money(decimal amount, Currency currency) => (Amount, Currency) = (Math.Round(amount, 2), currency);

    public static Money Zero => new(0, Currency.Empty);

    public bool IsZero => Amount == 0 && Currency == Currency.Empty;

    public Money Add(Money other) => IsZero
        ? other
        : other.IsZero
            ? this
            : Currency == other.Currency
                ? new Money(Amount + other.Amount, Currency)
                : throw new InvalidOperationException("Cannot add money of different currency.");

    public Money Subtract(Money other) => IsZero
        ? new Money(-other.Amount, Currency)
        : other.IsZero
            ? this
            : Currency == other.Currency
                ? new Money(Amount - other.Amount, Currency)
                : throw new InvalidOperationException("Cannot subtract money of different currency.");

    public Money Scale(decimal coefficient) => coefficient < 0
        ? throw new InvalidOperationException("Cannot multiple by a negative coefficient.")
        : new Money(Amount * coefficient, Currency);

    public int CompareTo(Money other)
    {
        var currenciesMatch =
            Currency == other.Currency
            || Currency == Currency.Empty
            || other.Currency == Currency.Empty;

        if (!currenciesMatch)
        {
            throw new InvalidOperationException("Cannot compare money of different currencies.");
        }

        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => right.Subtract(left);

    public static Money operator *(Money left, decimal coefficient) => left.Scale(coefficient);

    public static Money operator *(decimal coefficient, Money right) => right.Scale(coefficient);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{Amount:0.00} {Currency.Symbol}";
}

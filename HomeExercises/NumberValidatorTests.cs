using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidatorTests
    {
        [TestCase(2, 2, TestName = "ScaleEqualToPrecision")]
        [TestCase(1, 2, TestName = "ScaleGreaterThanPrecision")]
        [TestCase(1, -1, TestName = "NegativeScale")]
        [TestCase(0, 2, TestName = "ZeroPrecision")]
        [TestCase(-1, 2, TestName = "NegativePrecision")]
        public void NumberValidatorTest_ThrowArgumentException_When(int precision, int scale)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale));
        }

        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("1.", ExpectedResult = false)]
        [TestCase(".0", ExpectedResult = false)]
        [TestCase("+.0", ExpectedResult = false)]
        [TestCase("-.0", ExpectedResult = false)]
        [TestCase("123-1", ExpectedResult = false)]
        [TestCase("123+1", ExpectedResult = false)]
        [TestCase("123-", ExpectedResult = false)]
        [TestCase("123+", ExpectedResult = false)]
        [TestCase("12a3", ExpectedResult = false)]
        [TestCase("123a", ExpectedResult = false)]
        [TestCase("a123", ExpectedResult = false)]
        [TestCase("a", ExpectedResult = false)]
        [TestCase("+-1", ExpectedResult = false)]
        [TestCase("-+1", ExpectedResult = false)]
        [TestCase("1", ExpectedResult = true)]
        [TestCase("-1", ExpectedResult = true)]
        [TestCase("+1", ExpectedResult = true)]
        [TestCase("123.12", ExpectedResult = true)]
        [TestCase("123,12", ExpectedResult = true)]
        [TestCase("+123.12", ExpectedResult = true)]
        [TestCase("-123.12", ExpectedResult = true)]
        
        public bool ValidateNumber(string number)
        {
            var bigPrecisionAndScale = new NumberValidator(precision : 100, scale : 99);
            return bigPrecisionAndScale.IsValidNumber(number);
        }

        [TestCase("12345", TestName = "Incorrect precision", ExpectedResult = false)]
        [TestCase("1234", TestName = "Correct precision", ExpectedResult = true)]
        [TestCase("+1234", TestName = "Incorrect precision because sign", ExpectedResult = false)]
        [TestCase("12.34", TestName = "Correct scale", ExpectedResult = true)]
        [TestCase("1.234", TestName = "Incorrect scale", ExpectedResult = false)]
        [TestCase("-1", TestName = "Negative int at only-positive mode", ExpectedResult = false)]
        [TestCase("-1.0", TestName = "Negative fraction at only-positive mode", ExpectedResult = false)]
        public bool ValidateNumberByLength(string number)
        {
            return new NumberValidator(precision : 4, scale : 2, onlyPositive : true).IsValidNumber(number);
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}

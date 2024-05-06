using MongoDB.Bson;
using mozgovichok.Models.Organisations;

namespace mozgovichok.Infrastructure
{
    public static class BalanceHandler
    {
        /// <summary>
        /// создаем объект баланса, без учета остатков прошлых периодов
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="tariff"></param>
        /// <returns></returns>
        public static Balance Count(Payment payment, Tariff tariff)
        {
            Balance balance = new Balance();
            tariff ??= new Tariff();
            payment ??= new Payment();

            balance.Id = ObjectId.GenerateNewId().ToString();
            balance.LastPaymentDate = payment.PaymentDate;
            balance.Value = payment.PaymentAmount;
            balance.Currency = payment.Currency;
            balance.NextPaymentDate = payment.PaymentDate.AddDays(tariff.TariffDaysDuration);
            balance.TariffExpiration = payment.PaymentDate.AddDays(tariff.TariffDaysDuration);
            balance.Comment = null;

            return balance;
        }

        /// <summary>
        /// создаем объект баланса, без учета остатков прошлых периодов c комментарием
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="tariff"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static Balance Count(Payment payment, Tariff tariff, string comment)
        {
            Balance balance = new Balance();
            tariff ??= new Tariff();
            payment ??= new Payment();

            balance.Id = ObjectId.GenerateNewId().ToString();
            balance.LastPaymentDate = payment.PaymentDate;
            balance.Value = payment.PaymentAmount;
            balance.Currency = payment.Currency;
            balance.NextPaymentDate = payment.PaymentDate.AddDays(tariff.TariffDaysDuration);
            balance.TariffExpiration = payment.PaymentDate.AddDays(tariff.TariffDaysDuration);
            balance.Comment = comment;

            return balance;
        }

        /// <summary>
        /// создаем объект баланса, c учетом предыдущего баланса
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="tariff"></param>
        /// <param name="prevBalance"></param>
        /// <returns></returns>
        public static Balance Count(Payment payment, Tariff tariff, Balance prevBalance)
        {
            Balance balance = new Balance();
            tariff ??= new Tariff();
            payment ??= new Payment();

            balance.Id = ObjectId.GenerateNewId().ToString();
            balance.LastPaymentDate = payment.PaymentDate;
            balance.Value = payment.PaymentAmount + prevBalance?.Value;
            balance.Currency = payment.Currency;
            balance.NextPaymentDate = prevBalance.NextPaymentDate.AddDays(tariff.TariffDaysDuration);
            balance.TariffExpiration = prevBalance.NextPaymentDate.AddDays(tariff.TariffDaysDuration);
            //if(prevBalance.Comment != null) { balance.Comment = prevBalance.Comment; }
            balance.Comment = null;

            return balance;
        }
    }
}

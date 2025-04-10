using CensusFieldSurvey.DataBase.Repositories;
using CensusFieldSurvey.DataBase;
using Coravel.Invocable;
using Gestao.Domain;
using Gestao.Domain.Enums;
using Gestao.Domain.Repositories;

namespace Gestao.Libraries.Queues
{
    public class FinancialTransactionRepeatInvocable : IInvocable, IInvocableWithPayload<FinancialTransaction>
    {
        private readonly IRepository<FinancialTransaction> _repository;

        public FinancialTransactionRepeatInvocable(IRepository<FinancialTransaction> repository)
        {
            _repository = repository;
        }

        public FinancialTransaction Payload { get; set; }

        public async Task Invoke()
        {
            int startPoint = 1;
            int countTransactionsSameGroup = await (_repository as FinancialTransactionRepository).GetCountTransactionsSameGroup(Payload.Id);

            await AssignRepeatGroupToPayload();

            if (countTransactionsSameGroup == 0)
            {
                await RegisterNewTransactions(startPoint);
            }
            else
            {
                await RegisterNewTransactions(countTransactionsSameGroup);
            }

            await TransactionsReduction(countTransactionsSameGroup);

            await RepeatTransactionsRemove(countTransactionsSameGroup);
        }

        private async Task AssignRepeatGroupToPayload()
        {
            if (Payload.Repeat != Recurrence.None)
            {
                Payload.RepeatGroup = Payload.Id;
                await _repository.Update(Payload);
            }
        }

        private async Task RepeatTransactionsRemove(int countTransactionsSameGroup)
        {
            if (Payload.Repeat == Recurrence.None && countTransactionsSameGroup > 1)
            {
                var transactions = await (_repository as FinancialTransactionRepository).GetTransactionsSameGroup(Payload.Id);
                for (int i = 2; i <= countTransactionsSameGroup; i++)
                {
                    await _repository.Remove(transactions.ElementAt(i - 1).Id);
                }
            }
        }

        private async Task TransactionsReduction(int countTransactionsSameGroup)
        {
            if (Payload.Repeat != Recurrence.None && countTransactionsSameGroup > Payload.RepeatTimes)
            {
                var transactions = await (_repository as FinancialTransactionRepository).GetTransactionsSameGroup(Payload.Id);
                for (int i = countTransactionsSameGroup; i > Payload.RepeatTimes; i--)
                {
                    await _repository.Remove(transactions.ElementAt(i - 1).Id);
                }
            }
        }

        private async Task RegisterNewTransactions(int startPoint)
        {
            if (Payload.Repeat != Recurrence.None)
            {
                var repeatTimes = Payload.RepeatTimes - 1;

                for (int i = startPoint; i <= repeatTimes; i++)
                {
                    var financial = new FinancialTransaction();
                    financial.TypeFinancialTransaction = Payload.TypeFinancialTransaction;
                    financial.Description = Payload.Description;
                    financial.ReferenceDate = IncrementDate(Payload.Repeat, i, Payload.ReferenceDate);
                    financial.DueDate = Payload.DueDate.HasValue ? IncrementDate(Payload.Repeat, i, Payload.DueDate.Value) : null;
                    financial.Amount = Payload.Amount;
                    financial.RepeatGroup = Payload.Id;
                    financial.Repeat = Recurrence.None;
                    financial.RepeatTimes = null;
                    financial.CreatedAt = DateTimeOffset.Now;

                    financial.CompanyId = Payload.CompanyId;
                    financial.AccountId = Payload.AccountId;
                    financial.CategoryId = Payload.CategoryId;

                    await  _repository.Add(financial);
                }
            }
        }

        private DateTimeOffset IncrementDate(Recurrence repeat, int count, DateTimeOffset date)
        {
            DateTimeOffset dateModified = date;
            switch (repeat)
            {
                case Recurrence.Weekly:
                    dateModified = date.AddDays(7 * count);
                    break;
                case Recurrence.Monthly:
                    dateModified = date.AddMonths(count);
                    break;
                case Recurrence.Yearly:
                    dateModified = date.AddYears(count);
                    break;
                default:
                    break;
            }
            return dateModified;
        }
    }
}

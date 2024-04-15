namespace Tgs.Payment.AlfaPay;

public static class AlfaPayConsts
{
    public const string DepositSubjectTemplate = "tgs.payment.{0}.deposit";
    public const string WithdrawSubjectTemplate = "tgs.payment.{0}.withdraw";
    public const string ScheduleRejectSubjectTemplate = "tgs.payment.{0}.deposit.schedule.reject";
    public const string QueryBalanceSubjectTemplate = "tgs.payment.{0}.balance";
}
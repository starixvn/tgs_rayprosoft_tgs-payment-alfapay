namespace Tgs.Payment.AlfaPay.MessageQueue;

public static class ProcessorTypes
{
    public static readonly Type DepositProcessorType = typeof(DepositProcessor);
    public static readonly Type WithdrawProcessorType = typeof(WithdrawProcessor);
}

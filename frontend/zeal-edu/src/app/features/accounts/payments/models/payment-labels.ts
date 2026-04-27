import { PaymentMethod, PaymentType } from './payment-enums';

export const paymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]: 'Cash',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
};

export const paymentTypeLabels: Record<PaymentType, string> = {
  [PaymentType.NotSet]: 'Not Set',
  [PaymentType.FullPayment]: 'Full Payment',
  [PaymentType.Installment]: 'Installment Plan',
};

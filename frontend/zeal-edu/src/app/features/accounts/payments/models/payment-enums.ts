export enum FeeType {
  Tuition = 'Tuition',
  Fine = 'Fine',
}

export enum PaymentMethod {
  Cash = 'Cash',
  BankTransfer = 'BankTransfer',
}

export enum PaymentStatus {
  Unpaid = 'Unpaid',
  Partial = 'Partial',
  Paid = 'Paid',
  Overdue = 'Overdue',
}

export enum PaymentType {
  NotSet = 'NotSet',
  FullPayment = 'FullPayment',
  Installment = 'Installment',
}

export enum InstallmentStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Overdue = 'Overdue',
  PartiallyPaid = 'PartiallyPaid',
}

export enum InstallmentFrequency {
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  BiYearly = 'BiYearly',
}

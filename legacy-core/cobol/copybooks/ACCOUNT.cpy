      *>****************************************************************
      *> ACCOUNT.cpy  -  Fixed-width account master record (80 bytes)
      *> Mainframe-style layout used by the CobraBridge legacy core.
      *> Field offsets are positional and MUST NOT change without a
      *> matching migration of ACCOUNTS.DAT.
      *>****************************************************************
       01  ACCOUNT-RECORD.
           05  ACCT-ID            PIC X(10).
           05  ACCT-NAME          PIC X(30).
           05  ACCT-TYPE          PIC X(02).
      *>       CH = checking / current,  SV = savings
           05  ACCT-BALANCE       PIC 9(09)V99.
      *>       11 display digits, implied 2-decimal point (cents)
           05  ACCT-STATUS        PIC X(01).
      *>       A = active,  C = closed,  F = frozen
           05  FILLER             PIC X(26).

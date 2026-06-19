      *>****************************************************************
      *> ACCTLIST.cbl
      *> CobraBridge legacy core - daily account batch.
      *>
      *> Reads the fixed-width account master (ACCOUNTS.DAT), accrues
      *> one day of interest on active SAVINGS accounts, prints a
      *> formatted report (ACCTRPT.TXT) and rewrites the master with
      *> the updated balances. This is the kind of nightly batch a
      *> 1985-era core banking system would run on a mainframe.
      *>****************************************************************
       IDENTIFICATION DIVISION.
       PROGRAM-ID. ACCTLIST.
       AUTHOR. COBRABRIDGE.

       ENVIRONMENT DIVISION.
       INPUT-OUTPUT SECTION.
       FILE-CONTROL.
           SELECT ACCT-FILE   ASSIGN TO "ACCOUNTS.DAT"
               ORGANIZATION IS LINE SEQUENTIAL
               FILE STATUS IS WS-ACCT-STATUS.
           SELECT RPT-FILE    ASSIGN TO "ACCTRPT.TXT"
               ORGANIZATION IS LINE SEQUENTIAL
               FILE STATUS IS WS-RPT-STATUS.

       DATA DIVISION.
       FILE SECTION.
       FD  ACCT-FILE.
           COPY ACCOUNT.

       FD  RPT-FILE.
       01  RPT-LINE               PIC X(120).

       WORKING-STORAGE SECTION.
       01  WS-FILE-STATUS.
           05  WS-ACCT-STATUS     PIC XX.
           05  WS-RPT-STATUS      PIC XX.

       01  WS-FLAGS.
           05  WS-EOF             PIC X VALUE "N".
               88  END-OF-FILE    VALUE "Y".

       01  WS-COUNTERS.
           05  WS-READ-CNT        PIC 9(06) VALUE 0.
           05  WS-ACTIVE-CNT      PIC 9(06) VALUE 0.
           05  WS-INTEREST-CNT    PIC 9(06) VALUE 0.

       01  WS-ACCRUAL.
           05  WS-DAILY-RATE      PIC 9V9(7) VALUE 0.0000411.
      *>       ~1.50% nominal annual rate / 365 days
           05  WS-INTEREST-AMT    PIC 9(09)V99 VALUE 0.

       01  WS-TOTALS.
           05  WS-TOTAL-BALANCE   PIC 9(13)V99 VALUE 0.
           05  WS-TOTAL-INTEREST  PIC 9(11)V99 VALUE 0.

      *> ---- report edit masks -------------------------------------
       01  WS-HDR-1.
           05  FILLER PIC X(40) VALUE
               "COBRABRIDGE  -  DAILY ACCOUNT BATCH".
       01  WS-HDR-2.
           05  FILLER PIC X(60) VALUE ALL "=".
       01  WS-COL-HDR.
           05  FILLER PIC X(12) VALUE "ACCOUNT".
           05  FILLER PIC X(32) VALUE "NAME".
           05  FILLER PIC X(06) VALUE "TYPE".
           05  FILLER PIC X(18) VALUE "       BALANCE".
           05  FILLER PIC X(16) VALUE "    INTEREST".

       01  WS-DETAIL.
           05  D-ID         PIC X(12).
           05  D-NAME       PIC X(32).
           05  D-TYPE       PIC X(06).
           05  D-BALANCE    PIC ZZZ,ZZZ,ZZ9.99.
           05  FILLER       PIC X(03) VALUE SPACES.
           05  D-INTEREST   PIC ZZ,ZZZ,ZZ9.99.

       01  WS-TOT-LINE.
           05  FILLER       PIC X(50) VALUE "TOTAL ASSETS UNDER MANAGEMENT:".
           05  T-BALANCE    PIC Z,ZZZ,ZZZ,ZZ9.99.

       PROCEDURE DIVISION.
       0000-MAIN.
           PERFORM 1000-INIT
           PERFORM 2000-PROCESS UNTIL END-OF-FILE
           PERFORM 3000-FINALIZE
           STOP RUN.

       1000-INIT.
           OPEN INPUT  ACCT-FILE
           IF WS-ACCT-STATUS NOT = "00"
               DISPLAY "FATAL: cannot open ACCOUNTS.DAT status="
                       WS-ACCT-STATUS
               MOVE 16 TO RETURN-CODE
               STOP RUN
           END-IF
           OPEN OUTPUT RPT-FILE
           WRITE RPT-LINE FROM WS-HDR-1
           WRITE RPT-LINE FROM WS-HDR-2
           WRITE RPT-LINE FROM WS-COL-HDR
           WRITE RPT-LINE FROM WS-HDR-2.

       2000-PROCESS.
           READ ACCT-FILE
               AT END SET END-OF-FILE TO TRUE
               NOT AT END PERFORM 2100-HANDLE-RECORD
           END-READ.

       2100-HANDLE-RECORD.
           ADD 1 TO WS-READ-CNT
           IF ACCT-STATUS = "A"
               ADD 1 TO WS-ACTIVE-CNT
               MOVE 0 TO WS-INTEREST-AMT
               IF ACCT-TYPE = "SV"
                   COMPUTE WS-INTEREST-AMT ROUNDED =
                       ACCT-BALANCE * WS-DAILY-RATE
                   ADD WS-INTEREST-AMT TO ACCT-BALANCE
                   ADD WS-INTEREST-AMT TO WS-TOTAL-INTEREST
                   ADD 1 TO WS-INTEREST-CNT
               END-IF
               ADD ACCT-BALANCE TO WS-TOTAL-BALANCE
               PERFORM 2200-WRITE-DETAIL
           END-IF.

       2200-WRITE-DETAIL.
           MOVE ACCT-ID        TO D-ID
           MOVE ACCT-NAME      TO D-NAME
           MOVE ACCT-TYPE      TO D-TYPE
           MOVE ACCT-BALANCE   TO D-BALANCE
           MOVE WS-INTEREST-AMT TO D-INTEREST
           WRITE RPT-LINE FROM WS-DETAIL.

       3000-FINALIZE.
           WRITE RPT-LINE FROM WS-HDR-2
           MOVE WS-TOTAL-BALANCE TO T-BALANCE
           WRITE RPT-LINE FROM WS-TOT-LINE
           CLOSE ACCT-FILE RPT-FILE
           DISPLAY "ACCTLIST done. read="    WS-READ-CNT
                   " active="                WS-ACTIVE-CNT
                   " interest-accrued="      WS-INTEREST-CNT.

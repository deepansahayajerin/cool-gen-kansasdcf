// Program: SC_TRAN_MAINTAIN_TRANSACTION, ID: 371743215, model: 746.
// Short name: SWETRANP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_TRAN_MAINTAIN_TRANSACTION.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScTranMaintainTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_TRAN_MAINTAIN_TRANSACTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScTranMaintainTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScTranMaintainTransaction.
  /// </summary>
  public ScTranMaintainTransaction(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      request #   Description
    // 12/10/95 Alan Hackler                Initial Development
    // 12/12/96 R. Marchman                 Add new security/next tran
    // ---------------------------------------------
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. Fixed 
    // the IF loop.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.Starting.Trancode = import.Starting.Trancode;
    export.SelectBy.SelectChar = import.SelectBy.SelectChar;
    export.SortBy.SelectChar = import.SortBy.SelectChar;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Transaction.Assign(import.Group.Item.Transaction);

        var field1 = GetField(export.Group.Item.Transaction, "screenId");

        field1.Protected = true;

        var field2 = GetField(export.Group.Item.Transaction, "trancode");

        field2.Protected = true;

        if (IsEmpty(export.Group.Item.Transaction.MenuInd))
        {
          export.Group.Update.Transaction.MenuInd = "N";
        }

        if (IsEmpty(export.Group.Item.Transaction.NextTranAuthorization))
        {
          export.Group.Update.Transaction.NextTranAuthorization = "Y";
        }

        if (AsChar(import.Group.Item.Common.SelectChar) == 'S')
        {
          export.HiddenSelected.Assign(import.Group.Item.Transaction);
        }

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "RETURN"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        goto Test;
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
        (global.Command, "DELETE"))
      {
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      if (Equal(global.Command, "DELETE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            MoveTransaction(export.Group.Item.Transaction, local.Transaction);

            if (ReadTransactionCommand())
            {
              var field1 = GetField(export.Group.Item.Transaction, "screenId");

              field1.Color = "red";
              field1.Intensity = Intensity.High;
              field1.Highlighting = Highlighting.ReverseVideo;
              field1.Protected = true;

              var field2 = GetField(export.Group.Item.Transaction, "trancode");

              field2.Color = "red";
              field2.Intensity = Intensity.High;
              field2.Highlighting = Highlighting.ReverseVideo;
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.Transaction, "desc");

              field3.Error = true;

              var field4 = GetField(export.Group.Item.Common, "selectChar");

              field4.Color = "red";
              field4.Intensity = Intensity.High;
              field4.Highlighting = Highlighting.ReverseVideo;
              field4.Protected = false;
              field4.Focused = true;

              ExitState = "SC0014_SCREEN_ID_N_USE_NO_DELETE";
            }
          }
        }
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Group.Item.Transaction.ScreenId))
            {
              var field = GetField(export.Group.Item.Transaction, "screenId");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }

            if (IsEmpty(export.Group.Item.Transaction.Trancode))
            {
              var field = GetField(export.Group.Item.Transaction, "trancode");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }

            if (IsEmpty(export.Group.Item.Transaction.Desc))
            {
              var field = GetField(export.Group.Item.Transaction, "desc");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
              }
            }

            if (AsChar(export.Group.Item.Transaction.MenuInd) == 'Y' || AsChar
              (export.Group.Item.Transaction.MenuInd) == 'N')
            {
            }
            else
            {
              var field = GetField(export.Group.Item.Transaction, "menuInd");

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
              }
            }

            if (AsChar(export.Group.Item.Transaction.NextTranAuthorization) == 'Y'
              || AsChar
              (export.Group.Item.Transaction.NextTranAuthorization) == 'N')
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.Transaction, "nextTranAuthorization");
                

              field.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
              }
            }

            if (Equal(global.Command, "ADD"))
            {
              // make sure the screen id is unique
              if (ReadTransaction1())
              {
                var field = GetField(export.Group.Item.Transaction, "screenId");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "SC0002_SCREEN_ID_AE";
                }
              }

              // make sure the trancode is unique
              if (ReadTransaction2())
              {
                var field = GetField(export.Group.Item.Transaction, "trancode");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "SC0042_TRANCODE_AE";
                }
              }
            }
          }
        }
      }
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The above security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. 
    // Fixed the IF loop.
    // ---------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();
    }
    else
    {
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (IsEmpty(export.SelectBy.SelectChar))
        {
          export.SelectBy.SelectChar = "S";
        }

        if (IsEmpty(export.SortBy.SelectChar))
        {
          export.SortBy.SelectChar = "S";
        }

        if (AsChar(export.SelectBy.SelectChar) == 'S' || AsChar
          (export.SelectBy.SelectChar) == 'T')
        {
        }
        else
        {
          var field = GetField(export.SelectBy, "selectChar");

          field.Error = true;

          ExitState = "SC0010_MUST_BE_S_OR_T";

          return;
        }

        if (AsChar(export.SortBy.SelectChar) == 'S' || AsChar
          (export.SortBy.SelectChar) == 'T')
        {
        }
        else
        {
          var field = GetField(export.SortBy, "selectChar");

          field.Error = true;

          ExitState = "SC0010_MUST_BE_S_OR_T";

          return;
        }

        if (AsChar(export.SelectBy.SelectChar) == 'S')
        {
          if (AsChar(export.SortBy.SelectChar) == 'S')
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadTransaction3())
            {
              export.Group.Update.Transaction.Assign(
                entities.ExistingTransaction);

              var field1 = GetField(export.Group.Item.Transaction, "screenId");

              field1.Protected = true;

              var field2 = GetField(export.Group.Item.Transaction, "trancode");

              field2.Protected = true;

              export.Group.Next();
            }
          }
          else
          {
            export.Group.Index = 0;
            export.Group.Clear();

            foreach(var item in ReadTransaction5())
            {
              export.Group.Update.Transaction.Assign(
                entities.ExistingTransaction);

              var field1 = GetField(export.Group.Item.Transaction, "screenId");

              field1.Protected = true;

              var field2 = GetField(export.Group.Item.Transaction, "trancode");

              field2.Protected = true;

              export.Group.Next();
            }
          }
        }
        else if (AsChar(export.SortBy.SelectChar) == 'S')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadTransaction4())
          {
            export.Group.Update.Transaction.
              Assign(entities.ExistingTransaction);

            var field1 = GetField(export.Group.Item.Transaction, "screenId");

            field1.Protected = true;

            var field2 = GetField(export.Group.Item.Transaction, "trancode");

            field2.Protected = true;

            export.Group.Next();
          }
        }
        else
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadTransaction6())
          {
            export.Group.Update.Transaction.
              Assign(entities.ExistingTransaction);

            var field1 = GetField(export.Group.Item.Transaction, "screenId");

            field1.Protected = true;

            var field2 = GetField(export.Group.Item.Transaction, "trancode");

            field2.Protected = true;

            export.Group.Next();
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseCreateTransactions();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.Transaction, "screenId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Transaction, "trancode");

                field2.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseUpdateTransactions();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.Transaction, "screenId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Transaction, "trancode");

                field2.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseDeleteTransactions();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field1 =
                  GetField(export.Group.Item.Transaction, "screenId");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Transaction, "trancode");

                field2.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveTransaction(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
  }

  private void UseCreateTransactions()
  {
    var useImport = new CreateTransactions.Import();
    var useExport = new CreateTransactions.Export();

    useImport.Transaction.Assign(export.Group.Item.Transaction);

    Call(CreateTransactions.Execute, useImport, useExport);
  }

  private void UseDeleteTransactions()
  {
    var useImport = new DeleteTransactions.Import();
    var useExport = new DeleteTransactions.Export();

    MoveTransaction(export.Group.Item.Transaction, useImport.Transaction);

    Call(DeleteTransactions.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseUpdateTransactions()
  {
    var useImport = new UpdateTransactions.Import();
    var useExport = new UpdateTransactions.Export();

    useImport.Transaction.Assign(export.Group.Item.Transaction);

    Call(UpdateTransactions.Execute, useImport, useExport);
  }

  private bool ReadTransaction1()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction1",
      (db, command) =>
      {
        db.
          SetString(command, "screenId", export.Group.Item.Transaction.ScreenId);
          
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private bool ReadTransaction2()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction2",
      (db, command) =>
      {
        db.
          SetString(command, "trancode", export.Group.Item.Transaction.Trancode);
          
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTransaction3()
  {
    return ReadEach("ReadTransaction3",
      (db, command) =>
      {
        db.SetString(command, "screenId", import.Starting.Trancode);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction4()
  {
    return ReadEach("ReadTransaction4",
      (db, command) =>
      {
        db.SetString(command, "trancode", import.Starting.Trancode);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction5()
  {
    return ReadEach("ReadTransaction5",
      (db, command) =>
      {
        db.SetString(command, "screenId", import.Starting.Trancode);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction6()
  {
    return ReadEach("ReadTransaction6",
      (db, command) =>
      {
        db.SetString(command, "trancode", import.Starting.Trancode);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.NextTranAuthorization =
          db.GetString(reader, 4);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadTransactionCommand()
  {
    entities.ExistingTransactionCommand.Populated = false;

    return Read("ReadTransactionCommand",
      (db, command) =>
      {
        db.SetString(command, "fkTrnTrancode", local.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransactionCommand.Id = db.GetInt32(reader, 0);
        entities.ExistingTransactionCommand.FkTrnScreenid =
          db.GetString(reader, 1);
        entities.ExistingTransactionCommand.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingTransactionCommand.FkCmdValue =
          db.GetString(reader, 3);
        entities.ExistingTransactionCommand.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Transaction.
      /// </summary>
      [JsonPropertyName("transaction")]
      public Transaction Transaction
      {
        get => transaction ??= new();
        set => transaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common common;
      private Transaction transaction;
    }

    /// <summary>
    /// A value of SelectBy.
    /// </summary>
    [JsonPropertyName("selectBy")]
    public Common SelectBy
    {
      get => selectBy ??= new();
      set => selectBy = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Transaction Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common selectBy;
    private Transaction starting;
    private Common sortBy;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Transaction.
      /// </summary>
      [JsonPropertyName("transaction")]
      public Transaction Transaction
      {
        get => transaction ??= new();
        set => transaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common common;
      private Transaction transaction;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Transaction HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
    }

    /// <summary>
    /// A value of SelectBy.
    /// </summary>
    [JsonPropertyName("selectBy")]
    public Common SelectBy
    {
      get => selectBy ??= new();
      set => selectBy = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Transaction Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Transaction hiddenSelected;
    private Common selectBy;
    private Transaction starting;
    private Common sortBy;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Transaction transaction;
    private Common found;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    /// <summary>
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

    private TransactionCommand existingTransactionCommand;
    private Transaction existingTransaction;
  }
#endregion
}

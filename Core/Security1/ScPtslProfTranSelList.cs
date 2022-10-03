// Program: SC_PTSL_PROF_TRAN_SEL_LIST, ID: 371720316, model: 746.
// Short name: SWEPTSLP
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
/// A program: SC_PTSL_PROF_TRAN_SEL_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScPtslProfTranSelList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_PTSL_PROF_TRAN_SEL_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScPtslProfTranSelList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScPtslProfTranSelList.
  /// </summary>
  public ScPtslProfTranSelList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      request #   Description
    // 12/10/95 Alan Hackler                Initial Development
    // 08/26/96 G. Lofton                   Corrected overflow
    //                                      
    // problem.
    // 12/12/96 R. Marchman                 Add new security/next tran
    // --------------------------------------------------------
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. Fixed 
    // the IF loop.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    MoveProfile(import.Profile, export.Profile);
    export.ReturnConfirmation.Flag = import.ReturnConfirmation.Flag;
    export.SortOrder.OneChar = import.SortOrder.OneChar;
    MoveTransaction(import.StartingFrom, export.StartingFrom);

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar =
          import.Group.Item.Common.SelectChar;
        export.Group.Update.Transaction.Assign(import.Group.Item.Transaction);

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case 'S':
              ++local.Count.Count;

              break;
            case ' ':
              break;
            case '*':
              break;
            default:
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              break;
          }
        }

        export.Group.CheckIndex();
      }

      import.Group.CheckIndex();
    }

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
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
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
      case "ADD":
        if (local.Count.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.HiddenSelected.Index = -1;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            ++export.HiddenSelected.Index;
            export.HiddenSelected.CheckSize();

            MoveTransaction(export.Group.Item.Transaction,
              export.HiddenSelected.Update.HiddenSelectedTransaction);
            export.HiddenSelected.Update.HiddenSelectedCommon.SelectChar = "S";
          }

          if (export.HiddenSelected.IsFull)
          {
            break;
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        if (ReadProfile())
        {
          MoveProfile(entities.ExistingProfile, export.Profile);
        }
        else
        {
          var field = GetField(export.Profile, "name");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;

          ExitState = "SC0015_PROFILE_NF";

          return;
        }

        switch(AsChar(export.SortOrder.OneChar))
        {
          case 'A':
            break;
          case 'D':
            break;
          case ' ':
            export.SortOrder.OneChar = "A";

            break;
          default:
            var field = GetField(export.SortOrder, "oneChar");

            field.Error = true;

            ExitState = "SC0038_MUST_BE_A_OR_D";

            return;
        }

        if (!IsEmpty(export.StartingFrom.ScreenId))
        {
          if (!IsEmpty(export.StartingFrom.Trancode))
          {
            var field1 = GetField(export.StartingFrom, "screenId");

            field1.Error = true;

            var field2 = GetField(export.StartingFrom, "trancode");

            field2.Error = true;

            ExitState = "SC0036_ONLY_ONE_STARTING_FIELD";

            return;
          }
        }

        if (!IsEmpty(export.StartingFrom.ScreenId))
        {
          if (AsChar(export.SortOrder.OneChar) == 'A')
          {
            // ****    Populate the group of TRANSACTIONS already used by the 
            // related PROFILE AUTHORIZATION
            local.OfSelectedTxns.Index = -1;

            foreach(var item in ReadTransaction1())
            {
              if (local.OfSelectedTxns.Index + 1 == Local
                .OfSelectedTxnsGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++local.OfSelectedTxns.Index;
              local.OfSelectedTxns.CheckSize();

              MoveTransaction(entities.ExistingTransaction,
                local.OfSelectedTxns.Update.Selected);
            }

            export.Group.Index = -1;

            foreach(var item in ReadTransaction2())
            {
              // no need to select menus, they are authorized to everyone. you 
              // can do nothing on a menu.
              if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
              {
                continue;
              }

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.Transaction.Assign(
                entities.ExistingTransaction);
            }
          }
          else
          {
            // ****    Populate the group of TRANSACTIONS already used by the 
            // related PROFILE AUTHORIZATION
            local.OfSelectedTxns.Index = -1;

            foreach(var item in ReadTransaction7())
            {
              if (local.OfSelectedTxns.Index + 1 == Local
                .OfSelectedTxnsGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++local.OfSelectedTxns.Index;
              local.OfSelectedTxns.CheckSize();

              MoveTransaction(entities.ExistingTransaction,
                local.OfSelectedTxns.Update.Selected);
            }

            // ****   Descending Screen ID
            export.Group.Index = -1;

            foreach(var item in ReadTransaction8())
            {
              // no need to select menus, they are authorized to everyone. you 
              // can do nothing on a menu.
              if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
              {
                continue;
              }

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.Transaction.Assign(
                entities.ExistingTransaction);
            }
          }
        }
        else
        {
          // ****   Screen ID is spaces
          if (!IsEmpty(export.StartingFrom.Trancode))
          {
            if (AsChar(export.SortOrder.OneChar) == 'A')
            {
              // ****    Populate the group of TRANSACTIONS already used by the 
              // related PROFILE AUTHORIZATION
              local.OfSelectedTxns.Index = -1;

              foreach(var item in ReadTransaction5())
              {
                if (local.OfSelectedTxns.Index + 1 == Local
                  .OfSelectedTxnsGroup.Capacity)
                {
                  ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                  break;
                }

                ++local.OfSelectedTxns.Index;
                local.OfSelectedTxns.CheckSize();

                MoveTransaction(entities.ExistingTransaction,
                  local.OfSelectedTxns.Update.Selected);
              }

              export.Group.Index = -1;

              foreach(var item in ReadTransaction6())
              {
                // no need to select menus, they are authorized to everyone. you
                // can do nothing on a menu.
                if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
                {
                  continue;
                }

                if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
                {
                  ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                  break;
                }

                ++export.Group.Index;
                export.Group.CheckSize();

                export.Group.Update.Transaction.Assign(
                  entities.ExistingTransaction);
              }
            }
            else
            {
              // ****    Populate the group of TRANSACTIONS already used by the 
              // related PROFILE AUTHORIZATION
              local.OfSelectedTxns.Index = -1;

              foreach(var item in ReadTransaction9())
              {
                if (local.OfSelectedTxns.Index + 1 == Local
                  .OfSelectedTxnsGroup.Capacity)
                {
                  ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                  break;
                }

                ++local.OfSelectedTxns.Index;
                local.OfSelectedTxns.CheckSize();

                MoveTransaction(entities.ExistingTransaction,
                  local.OfSelectedTxns.Update.Selected);
              }

              // ****   Tranaction ID is spaces; Descending Trancode order
              export.Group.Index = -1;

              foreach(var item in ReadTransaction10())
              {
                // no need to select menus, they are authorized to everyone. you
                // can do nothing on a menu.
                if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
                {
                  continue;
                }

                if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
                {
                  ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                  break;
                }

                ++export.Group.Index;
                export.Group.CheckSize();

                export.Group.Update.Transaction.Assign(
                  entities.ExistingTransaction);
              }
            }
          }
          else
          {
            // ****    Populate the group of TRANSACTIONS already used by the 
            // related PROFILE AUTHORIZATION
            local.OfSelectedTxns.Index = -1;

            foreach(var item in ReadTransaction3())
            {
              if (local.OfSelectedTxns.Index + 1 == Local
                .OfSelectedTxnsGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++local.OfSelectedTxns.Index;
              local.OfSelectedTxns.CheckSize();

              MoveTransaction(entities.ExistingTransaction,
                local.OfSelectedTxns.Update.Selected);
            }

            // ****    Screen ID and Trancode spaces
            export.Group.Index = -1;

            foreach(var item in ReadTransaction4())
            {
              // no need to select menus, they are authorized to everyone. you 
              // can do nothing on a menu.
              if (AsChar(entities.ExistingTransaction.MenuInd) == 'Y')
              {
                continue;
              }

              if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
              {
                ExitState = "SC0044_VIEW_EXCEEDED_USE_STRT_KY";

                break;
              }

              ++export.Group.Index;
              export.Group.CheckSize();

              export.Group.Update.Transaction.Assign(
                entities.ExistingTransaction);
            }
          }
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          for(local.OfSelectedTxns.Index = 0; local.OfSelectedTxns.Index < local
            .OfSelectedTxns.Count; ++local.OfSelectedTxns.Index)
          {
            if (!local.OfSelectedTxns.CheckSize())
            {
              break;
            }

            if (Equal(export.Group.Item.Transaction.ScreenId,
              local.OfSelectedTxns.Item.Selected.ScreenId) && Equal
              (export.Group.Item.Transaction.Trancode,
              local.OfSelectedTxns.Item.Selected.Trancode))
            {
              export.Group.Update.Common.SelectChar = "*";

              goto Next;
            }
          }

          local.OfSelectedTxns.CheckIndex();

Next:
          ;
        }

        export.Group.CheckIndex();
        export.ReturnConfirmation.Flag = "";

        break;
      case "RETURN":
        if (AsChar(export.ReturnConfirmation.Flag) == 'Y')
        {
        }
        else if (local.Count.Count > 0)
        {
          export.ReturnConfirmation.Flag = "Y";
          ExitState = "SC0032_CONFIRM_RETURN_IGNR_SEL";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

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

  private static void MoveProfile(Profile source, Profile target)
  {
    target.Name = source.Name;
    target.Desc = source.Desc;
  }

  private static void MoveTransaction(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
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

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", export.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private IEnumerable<bool> ReadTransaction1()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction1",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "screenId", export.StartingFrom.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction10()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction10",
      (db, command) =>
      {
        db.SetString(command, "trancode", export.StartingFrom.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction2()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction2",
      (db, command) =>
      {
        db.SetString(command, "screenId", export.StartingFrom.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction3()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction3",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction4()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction4",
      null,
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction5()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction5",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "screenId", export.StartingFrom.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction6()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction6",
      (db, command) =>
      {
        db.SetString(command, "trancode", export.StartingFrom.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction7()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction7",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "screenId", export.StartingFrom.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction8()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction8",
      (db, command) =>
      {
        db.SetString(command, "screenId", export.StartingFrom.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTransaction9()
  {
    entities.ExistingTransaction.Populated = false;

    return ReadEach("ReadTransaction9",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(command, "screenId", export.StartingFrom.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;

        return true;
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
      public const int Capacity = 250;

      private Common common;
      private Transaction transaction;
    }

    /// <summary>
    /// A value of ReturnConfirmation.
    /// </summary>
    [JsonPropertyName("returnConfirmation")]
    public Common ReturnConfirmation
    {
      get => returnConfirmation ??= new();
      set => returnConfirmation = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of SortOrder.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public Standard SortOrder
    {
      get => sortOrder ??= new();
      set => sortOrder = value;
    }

    /// <summary>
    /// A value of StartingFrom.
    /// </summary>
    [JsonPropertyName("startingFrom")]
    public Transaction StartingFrom
    {
      get => startingFrom ??= new();
      set => startingFrom = value;
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

    private Common returnConfirmation;
    private Profile profile;
    private Array<GroupGroup> group;
    private Standard sortOrder;
    private Transaction startingFrom;
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
      public const int Capacity = 250;

      private Common common;
      private Transaction transaction;
    }

    /// <summary>A HiddenSelectedGroup group.</summary>
    [Serializable]
    public class HiddenSelectedGroup
    {
      /// <summary>
      /// A value of HiddenSelectedCommon.
      /// </summary>
      [JsonPropertyName("hiddenSelectedCommon")]
      public Common HiddenSelectedCommon
      {
        get => hiddenSelectedCommon ??= new();
        set => hiddenSelectedCommon = value;
      }

      /// <summary>
      /// A value of HiddenSelectedTransaction.
      /// </summary>
      [JsonPropertyName("hiddenSelectedTransaction")]
      public Transaction HiddenSelectedTransaction
      {
        get => hiddenSelectedTransaction ??= new();
        set => hiddenSelectedTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common hiddenSelectedCommon;
      private Transaction hiddenSelectedTransaction;
    }

    /// <summary>
    /// A value of ReturnConfirmation.
    /// </summary>
    [JsonPropertyName("returnConfirmation")]
    public Common ReturnConfirmation
    {
      get => returnConfirmation ??= new();
      set => returnConfirmation = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of SortOrder.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public Standard SortOrder
    {
      get => sortOrder ??= new();
      set => sortOrder = value;
    }

    /// <summary>
    /// A value of StartingFrom.
    /// </summary>
    [JsonPropertyName("startingFrom")]
    public Transaction StartingFrom
    {
      get => startingFrom ??= new();
      set => startingFrom = value;
    }

    /// <summary>
    /// Gets a value of HiddenSelected.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSelectedGroup> HiddenSelected => hiddenSelected ??= new(
      HiddenSelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenSelected for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    [Computed]
    public IList<HiddenSelectedGroup> HiddenSelected_Json
    {
      get => hiddenSelected;
      set => HiddenSelected.Assign(value);
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

    private Common returnConfirmation;
    private Profile profile;
    private Array<GroupGroup> group;
    private Standard sortOrder;
    private Transaction startingFrom;
    private Array<HiddenSelectedGroup> hiddenSelected;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A OfSelectedTxnsGroup group.</summary>
    [Serializable]
    public class OfSelectedTxnsGroup
    {
      /// <summary>
      /// A value of Selected.
      /// </summary>
      [JsonPropertyName("selected")]
      public Transaction Selected
      {
        get => selected ??= new();
        set => selected = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 250;

      private Transaction selected;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public Transaction Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// Gets a value of OfSelectedTxns.
    /// </summary>
    [JsonIgnore]
    public Array<OfSelectedTxnsGroup> OfSelectedTxns => ofSelectedTxns ??= new(
      OfSelectedTxnsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OfSelectedTxns for json serialization.
    /// </summary>
    [JsonPropertyName("ofSelectedTxns")]
    [Computed]
    public IList<OfSelectedTxnsGroup> OfSelectedTxns_Json
    {
      get => ofSelectedTxns;
      set => OfSelectedTxns.Assign(value);
    }

    private Transaction clear;
    private Common count;
    private Array<OfSelectedTxnsGroup> ofSelectedTxns;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
    }

    /// <summary>
    /// A value of ExistingProfileAuthorization.
    /// </summary>
    [JsonPropertyName("existingProfileAuthorization")]
    public ProfileAuthorization ExistingProfileAuthorization
    {
      get => existingProfileAuthorization ??= new();
      set => existingProfileAuthorization = value;
    }

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

    private Profile existingProfile;
    private ProfileAuthorization existingProfileAuthorization;
    private TransactionCommand existingTransactionCommand;
    private Transaction existingTransaction;
  }
#endregion
}

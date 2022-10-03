// Program: SI_QUCK_QUICK_AUDIT_SCREEN, ID: 374544782, model: 746.
// Short name: SWEQUCKP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUCK_QUICK_AUDIT_SCREEN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiQuckQuickAuditScreen: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUCK_QUICK_AUDIT_SCREEN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuckQuickAuditScreen(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuckQuickAuditScreen.
  /// </summary>
  public SiQuckQuickAuditScreen(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***   development work and update history   ***
    // This program is for the online screen QUCK which displays information
    // from the srckt41.ckt_quick_audit table.  This is part of the Query Cases
    // for Kids system, a branch of CSENet  which is accessed through the State
    // Services Portal.
    // ---------------------------------------------------------------------------------------------
    // Date             Developer        Request#             Description
    // ---------------------------------------------------------------------------------------------
    // 09/03/09          Hockman        cq# 211          Initial development 
    // during Quick project
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.Search.EndDate = local.Null1.Date;
    export.Search.StartDate = local.Null1.Date;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.NextTranInfo);
    export.Search.Assign(import.Search);

    // ---------------------------------------------------------------------------
    // If the next tran is not spaces the user has requested a
    //  next tran action
    // ---------------------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
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

    // ----------------------------------------------------------------------------------
    // user has entered this screen from another screen
    // ----------------------------------------------------------------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      export.Hidden.Assign(local.NextTranInfo);

      return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }

    export.ReqStatePrompt.SelectChar = import.ReqStatePrompt.SelectChar;
    export.RequestedDataPrompt.SelectChar =
      import.RequestedDataPrompt.SelectChar;
    export.SelectedFips.Assign(import.SelectedFips);
    export.DisplayOnly.Assign(import.DisplayOnly);

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.ReqStatePrompt.SelectChar = "";
      export.RequestedDataPrompt.SelectChar = "";
    }

    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETFIPL":
        if (!IsEmpty(import.ReqStatePrompt.SelectChar))
        {
          if (AsChar(import.ReqStatePrompt.SelectChar) == 'S')
          {
            export.ReqStatePrompt.SelectChar = "";
            export.RequestedDataPrompt.SelectChar = "";

            if (import.SelectedFips.State > 0 && import.SelectedFips.State != export
              .SelectedFips.State)
            {
              export.SelectedFips.State = import.SelectedFips.State;
              export.SelectedFips.StateDescription =
                import.SelectedFips.StateDescription ?? "";
            }
          }
          else
          {
            export.ReqStatePrompt.SelectChar = "";
          }
        }

        break;
      case "RETCDVL":
        if (!IsEmpty(import.RequestedDataPrompt.SelectChar))
        {
          if (AsChar(import.RequestedDataPrompt.SelectChar) == 'S')
          {
            export.RequestedDataPrompt.SelectChar = "";
            export.ReqStatePrompt.SelectChar = "";

            if (!IsEmpty(import.SelectedCodeValue.Cdvalue))
            {
              switch(TrimEnd(import.SelectedCodeValue.Cdvalue))
              {
                case "CONTACT IN":
                  export.Search.DataRequestType = "CONTACTINFORMATION";

                  break;
                case "CASE PART":
                  export.Search.DataRequestType = "CASEPARTICIPANTS";

                  break;
                case "CASE ACTIV":
                  export.Search.DataRequestType = "CASEACTIVITY";

                  break;
                default:
                  export.Search.DataRequestType =
                    import.SelectedCodeValue.Cdvalue;

                  break;
              }
            }
          }
          else
          {
            export.RequestedDataPrompt.SelectChar = "";
          }
        }

        break;
      case "PREV":
        ExitState = "FN0000_NO_RECORDS_FOUND";

        foreach(var item in ReadQuickAudit2())
        {
          if (!IsEmpty(export.Search.DataRequestType))
          {
            if (!Equal(export.Search.DataRequestType,
              entities.QuickAudit.DataRequestType))
            {
              continue;
            }
          }

          if (export.SelectedFips.State > 0)
          {
            if (export.SelectedFips.State != StringToNumber
              (Substring(
                entities.QuickAudit.RequestorId,
              QuickAudit.RequestorId_MaxLength, 1, 2)))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.RequestingCaseId))
          {
            if (!Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseId) && !
              Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseOtherId))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.SystemUserId))
          {
            if (!Equal(export.Search.SystemUserId,
              entities.QuickAudit.SystemUserId))
            {
              continue;
            }
          }

          if (!Lt(Date(entities.QuickAudit.RequestTimestamp),
            export.Search.StartDate) && !
            Lt(export.Search.EndDate, Date(entities.QuickAudit.RequestTimestamp)))
            
          {
          }
          else
          {
            continue;
          }

          export.DisplayOnly.Assign(entities.QuickAudit);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          export.Scrolling.Text12 = "More - +";

          break;
        }

        if (IsExitState("FN0000_NO_RECORDS_FOUND") && !
          IsEmpty(export.DisplayOnly.DataRequestType))
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";
          export.Scrolling.Text12 = "More +";
        }

        break;
      case "NEXT":
        ExitState = "FN0000_NO_RECORDS_FOUND";

        foreach(var item in ReadQuickAudit1())
        {
          if (!IsEmpty(export.Search.DataRequestType))
          {
            if (!Equal(export.Search.DataRequestType,
              entities.QuickAudit.DataRequestType))
            {
              continue;
            }
          }

          if (export.SelectedFips.State > 0)
          {
            if (export.SelectedFips.State != StringToNumber
              (Substring(
                entities.QuickAudit.RequestorId,
              QuickAudit.RequestorId_MaxLength, 1, 2)))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.RequestingCaseId))
          {
            if (!Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseId) && !
              Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseOtherId))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.SystemUserId))
          {
            if (!Equal(export.Search.SystemUserId,
              entities.QuickAudit.SystemUserId))
            {
              continue;
            }
          }

          if (!Lt(Date(entities.QuickAudit.RequestTimestamp),
            export.Search.StartDate) && !
            Lt(export.Search.EndDate, Date(entities.QuickAudit.RequestTimestamp)))
            
          {
          }
          else
          {
            continue;
          }

          export.DisplayOnly.Assign(entities.QuickAudit);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          export.Scrolling.Text12 = "More - +";

          break;
        }

        if (IsExitState("FN0000_NO_RECORDS_FOUND") && !
          IsEmpty(export.DisplayOnly.DataRequestType))
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          export.Scrolling.Text12 = "More -";
        }

        break;
      case "LIST":
        local.Common.Count = 0;

        if (AsChar(export.RequestedDataPrompt.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        if (AsChar(export.ReqStatePrompt.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            if (!IsEmpty(import.RequestedDataPrompt.SelectChar))
            {
              var field = GetField(export.RequestedDataPrompt, "selectChar");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field = GetField(export.ReqStatePrompt, "selectChar");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;
            }

            break;
          case 1:
            if (!IsEmpty(export.RequestedDataPrompt.SelectChar))
            {
              if (AsChar(export.RequestedDataPrompt.SelectChar) == 'S')
              {
                export.Prompt.CodeName = "DATA REQUEST TYPE";
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                return;
              }
            }

            if (!IsEmpty(import.ReqStatePrompt.SelectChar) && AsChar
              (import.ReqStatePrompt.SelectChar) == 'S')
            {
              ExitState = "ECO_LNK_TO_LIST_FIPS";
            }

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            var field1 = GetField(export.RequestedDataPrompt, "selectChar");

            field1.Color = "red";
            field1.Protected = false;
            field1.Focused = true;

            var field2 = GetField(export.ReqStatePrompt, "selectChar");

            field2.Color = "red";
            field2.Protected = false;
            field2.Focused = false;

            break;
        }

        break;
      case "DISPLAY":
        export.DisplayOnly.DataRequestType =
          Spaces(QuickAudit.DataRequestType_MaxLength);
        export.DisplayOnly.RequestTimestamp = local.Blank.Timestamp;
        export.DisplayOnly.RequestingCaseId = "";
        export.DisplayOnly.RequestingCaseOtherId = "";
        export.DisplayOnly.DataResponseCode = "";
        export.DisplayOnly.DataResponseMessage =
          Spaces(QuickAudit.DataResponseMessage_MaxLength);
        export.DisplayOnly.EndDate = local.Null1.Date;
        export.DisplayOnly.ProviderCaseOtherId =
          Spaces(QuickAudit.ProviderCaseOtherId_MaxLength);
        export.DisplayOnly.ProviderCaseState =
          Spaces(QuickAudit.ProviderCaseState_MaxLength);
        export.DisplayOnly.RequestorId = "";
        export.DisplayOnly.RequestingCaseState =
          Spaces(QuickAudit.RequestingCaseState_MaxLength);
        export.DisplayOnly.StartDate = local.Blank.Date;
        export.DisplayOnly.StateGeneratedId =
          Spaces(QuickAudit.StateGeneratedId_MaxLength);
        export.DisplayOnly.SystemResponseCode = "";
        export.DisplayOnly.SystemResponseMessage =
          Spaces(QuickAudit.SystemResponseMessage_MaxLength);
        export.DisplayOnly.SystemServerId = "";
        export.DisplayOnly.SystemUserId = "";
        export.SelectedFips.StateDescription = "";

        if (import.SelectedFips.State > 0)
        {
          if (ReadFips())
          {
            MoveFips(entities.Fips, export.SelectedFips);
          }
          else
          {
            var field = GetField(export.SelectedFips, "state");

            field.Error = true;

            ExitState = "FIPS_NF";

            return;
          }
        }

        if (!IsEmpty(export.Search.DataRequestType))
        {
          switch(TrimEnd(export.Search.DataRequestType))
          {
            case "CASEACTIVITY":
              break;
            case "CASEPARTICIPANTS":
              break;
            case "CONTACTINFORMATION":
              break;
            case "FINANCIAL":
              break;
            default:
              ExitState = "INVALID_VALUE";

              var field = GetField(export.Search, "dataRequestType");

              field.Error = true;

              return;
          }
        }

        if (Equal(export.Search.StartDate, local.Null1.Date))
        {
          export.Search.StartDate = Now().Date;
        }

        if (Equal(export.Search.EndDate, local.Null1.Date))
        {
          export.Search.EndDate = Now().Date;
        }

        if (Lt(export.Search.EndDate, export.Search.StartDate))
        {
          var field = GetField(export.Search, "endDate");

          field.Error = true;

          ExitState = "ACO_NE0000_END_LESS_THAN_START";

          return;
        }

        ExitState = "FN0000_NO_RECORDS_FOUND";

        foreach(var item in ReadQuickAudit1())
        {
          if (!IsEmpty(export.Search.DataRequestType))
          {
            if (!Equal(export.Search.DataRequestType,
              entities.QuickAudit.DataRequestType))
            {
              continue;
            }
          }

          if (export.SelectedFips.State > 0)
          {
            if (export.SelectedFips.State != StringToNumber
              (Substring(
                entities.QuickAudit.RequestorId,
              QuickAudit.RequestorId_MaxLength, 1, 2)))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.RequestingCaseId))
          {
            if (!Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseId) && !
              Equal(export.Search.RequestingCaseId,
              entities.QuickAudit.RequestingCaseOtherId))
            {
              continue;
            }
          }

          if (!IsEmpty(export.Search.SystemUserId))
          {
            if (!Equal(export.Search.SystemUserId,
              entities.QuickAudit.SystemUserId))
            {
              continue;
            }
          }

          if (!Lt(Date(entities.QuickAudit.RequestTimestamp),
            export.Search.StartDate) && !
            Lt(export.Search.EndDate, Date(entities.QuickAudit.RequestTimestamp)))
            
          {
          }
          else
          {
            continue;
          }

          export.DisplayOnly.Assign(entities.QuickAudit);
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          export.Scrolling.Text12 = "More";

          foreach(var item1 in ReadQuickAudit1())
          {
            if (!IsEmpty(export.Search.DataRequestType))
            {
              if (!Equal(export.Search.DataRequestType,
                entities.QuickAudit.DataRequestType))
              {
                continue;
              }
            }

            if (export.SelectedFips.State > 0)
            {
              if (export.SelectedFips.State != StringToNumber
                (Substring(
                  entities.QuickAudit.RequestorId,
                QuickAudit.RequestorId_MaxLength, 1, 2)))
              {
                continue;
              }
            }

            if (!IsEmpty(export.Search.RequestingCaseId))
            {
              if (!Equal(export.Search.RequestingCaseId,
                entities.QuickAudit.RequestingCaseId) && !
                Equal(export.Search.RequestingCaseId,
                entities.QuickAudit.RequestingCaseOtherId))
              {
                continue;
              }
            }

            if (!IsEmpty(export.Search.SystemUserId))
            {
              if (!Equal(export.Search.SystemUserId,
                entities.QuickAudit.SystemUserId))
              {
                continue;
              }
            }

            if (!Lt(Date(entities.QuickAudit.RequestTimestamp),
              export.Search.StartDate) && !
              Lt(export.Search.EndDate,
              Date(entities.QuickAudit.RequestTimestamp)))
            {
            }
            else
            {
              continue;
            }

            export.Scrolling.Text12 = "More +";

            break;
          }

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.State = source.State;
    target.StateDescription = source.StateDescription;
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

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.Hidden);

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

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.SelectedFips.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateDescription = db.GetNullableString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadQuickAudit1()
  {
    entities.QuickAudit.Populated = false;

    return ReadEach("ReadQuickAudit1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "requestTimestamp",
          export.DisplayOnly.RequestTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.QuickAudit.SystemUserId = db.GetString(reader, 0);
        entities.QuickAudit.RequestTimestamp = db.GetDateTime(reader, 1);
        entities.QuickAudit.RequestorId = db.GetString(reader, 2);
        entities.QuickAudit.RequestingCaseId = db.GetString(reader, 3);
        entities.QuickAudit.RequestingCaseOtherId = db.GetString(reader, 4);
        entities.QuickAudit.SystemServerId = db.GetString(reader, 5);
        entities.QuickAudit.SystemResponseCode = db.GetString(reader, 6);
        entities.QuickAudit.DataResponseCode = db.GetString(reader, 7);
        entities.QuickAudit.StartDate = db.GetNullableDate(reader, 8);
        entities.QuickAudit.EndDate = db.GetNullableDate(reader, 9);
        entities.QuickAudit.DataRequestType = db.GetString(reader, 10);
        entities.QuickAudit.ProviderCaseState = db.GetString(reader, 11);
        entities.QuickAudit.ProviderCaseOtherId =
          db.GetNullableString(reader, 12);
        entities.QuickAudit.RequestingCaseState = db.GetString(reader, 13);
        entities.QuickAudit.StateGeneratedId = db.GetString(reader, 14);
        entities.QuickAudit.SystemResponseMessage = db.GetString(reader, 15);
        entities.QuickAudit.DataResponseMessage = db.GetString(reader, 16);
        entities.QuickAudit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadQuickAudit2()
  {
    entities.QuickAudit.Populated = false;

    return ReadEach("ReadQuickAudit2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "requestTimestamp",
          export.DisplayOnly.RequestTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.QuickAudit.SystemUserId = db.GetString(reader, 0);
        entities.QuickAudit.RequestTimestamp = db.GetDateTime(reader, 1);
        entities.QuickAudit.RequestorId = db.GetString(reader, 2);
        entities.QuickAudit.RequestingCaseId = db.GetString(reader, 3);
        entities.QuickAudit.RequestingCaseOtherId = db.GetString(reader, 4);
        entities.QuickAudit.SystemServerId = db.GetString(reader, 5);
        entities.QuickAudit.SystemResponseCode = db.GetString(reader, 6);
        entities.QuickAudit.DataResponseCode = db.GetString(reader, 7);
        entities.QuickAudit.StartDate = db.GetNullableDate(reader, 8);
        entities.QuickAudit.EndDate = db.GetNullableDate(reader, 9);
        entities.QuickAudit.DataRequestType = db.GetString(reader, 10);
        entities.QuickAudit.ProviderCaseState = db.GetString(reader, 11);
        entities.QuickAudit.ProviderCaseOtherId =
          db.GetNullableString(reader, 12);
        entities.QuickAudit.RequestingCaseState = db.GetString(reader, 13);
        entities.QuickAudit.StateGeneratedId = db.GetString(reader, 14);
        entities.QuickAudit.SystemResponseMessage = db.GetString(reader, 15);
        entities.QuickAudit.DataResponseMessage = db.GetString(reader, 16);
        entities.QuickAudit.Populated = true;

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
    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
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

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of ReqStatePrompt.
    /// </summary>
    [JsonPropertyName("reqStatePrompt")]
    public Common ReqStatePrompt
    {
      get => reqStatePrompt ??= new();
      set => reqStatePrompt = value;
    }

    /// <summary>
    /// A value of RequestedDataPrompt.
    /// </summary>
    [JsonPropertyName("requestedDataPrompt")]
    public Common RequestedDataPrompt
    {
      get => requestedDataPrompt ??= new();
      set => requestedDataPrompt = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public QuickAudit Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public QuickAudit DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
    }

    private Fips selectedFips;
    private CodeValue selectedCodeValue;
    private Standard standard;
    private NextTranInfo nextTranInfo;
    private Common reqStatePrompt;
    private Common requestedDataPrompt;
    private QuickAudit search;
    private QuickAudit displayOnly;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public TextWorkArea Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of SelectedFips.
    /// </summary>
    [JsonPropertyName("selectedFips")]
    public Fips SelectedFips
    {
      get => selectedFips ??= new();
      set => selectedFips = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Code Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of SelectedCodeValue.
    /// </summary>
    [JsonPropertyName("selectedCodeValue")]
    public CodeValue SelectedCodeValue
    {
      get => selectedCodeValue ??= new();
      set => selectedCodeValue = value;
    }

    /// <summary>
    /// A value of DisplayOnly.
    /// </summary>
    [JsonPropertyName("displayOnly")]
    public QuickAudit DisplayOnly
    {
      get => displayOnly ??= new();
      set => displayOnly = value;
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
    /// A value of ReqStatePrompt.
    /// </summary>
    [JsonPropertyName("reqStatePrompt")]
    public Common ReqStatePrompt
    {
      get => reqStatePrompt ??= new();
      set => reqStatePrompt = value;
    }

    /// <summary>
    /// A value of RequestedDataPrompt.
    /// </summary>
    [JsonPropertyName("requestedDataPrompt")]
    public Common RequestedDataPrompt
    {
      get => requestedDataPrompt ??= new();
      set => requestedDataPrompt = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public QuickAudit Search
    {
      get => search ??= new();
      set => search = value;
    }

    private TextWorkArea scrolling;
    private Fips selectedFips;
    private Code prompt;
    private CodeValue selectedCodeValue;
    private QuickAudit displayOnly;
    private Standard standard;
    private NextTranInfo hidden;
    private Common reqStatePrompt;
    private Common requestedDataPrompt;
    private QuickAudit search;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    private DateWorkArea blank;
    private DateWorkArea null1;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private Fips fips;
    private QuickAudit quickAudit;
  }
#endregion
}

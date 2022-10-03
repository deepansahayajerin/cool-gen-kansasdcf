// Program: SI_ILOC_INTERSTATE_AP_LOC_COMP, ID: 372541651, model: 746.
// Short name: SWEILOCP
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
/// <para>
/// A program: SI_ILOC_INTERSTATE_AP_LOC_COMP.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIlocInterstateApLocComp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ILOC_INTERSTATE_AP_LOC_COMP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIlocInterstateApLocComp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIlocInterstateApLocComp.
  /// </summary>
  public SiIlocInterstateApLocComp(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date	  Developer Name	Description
    // 05-22-95  Ken Evans - MTW	Initial development
    // 02-01-96  J. Howard - SRS	Retrofit
    // 11/04/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 4/12/99   C. Ott                Added Interstate Case transaction date to
    //                                 
    // views to fully qualify reads.
    // 4/22/99   C. Ott                Modifications fo screen display and exit
    //                                 
    // state messages
    // ------------------------------------------------------------
    // ***************************************************************
    // 9/10/99   C. Ott    Removed asterisks from selection field after 
    // successful update.
    // ***************************************************************
    // *********************************************
    // This procedure will display the incoming
    // CSENET data and the equivilent data in the
    // database. If the CSENET data is more current,
    // then the user selects the field(s) and PF6
    // (update).
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.InterstateCase.Assign(import.InterstateCase);
    export.CsePerson.Assign(import.CsePerson);
    export.CsePersonLicense.Assign(import.CsePersonLicense);
    export.InterstateApLocate.Assign(import.InterstateApLocate);
    export.Dl.SelectChar = import.Dl.SelectChar;
    export.DlSt.SelectChar = import.DlSt.SelectChar;
    export.HomePh.SelectChar = import.HomePh.SelectChar;
    export.Occup.SelectChar = import.Occup.SelectChar;
    export.SpFn.SelectChar = import.SpFn.SelectChar;
    export.SpLn.SelectChar = import.SpLn.SelectChar;
    export.SpMi.SelectChar = import.SpMi.SelectChar;
    export.WorkPh.SelectChar = import.WorkPh.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!import.PartList.IsEmpty)
    {
      export.PartList.Index = -1;

      for(import.PartList.Index = 0; import.PartList.Index < import
        .PartList.Count; ++import.PartList.Index)
      {
        if (!import.PartList.CheckSize())
        {
          break;
        }

        ++export.PartList.Index;
        export.PartList.CheckSize();

        export.PartList.Update.G.Assign(import.PartList.Item.G);
      }

      import.PartList.CheckIndex();
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the next_tran_info attributes to the import
      // view attributes for the data to be passed to the next transaction
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "IAPC":
        break;
      case "IAPH":
        break;
      case "ISUP":
        break;
      case "IMIS":
        break;
      case "SCNX":
        break;
      default:
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Dl.SelectChar = "";
        export.DlSt.SelectChar = "";
        export.HomePh.SelectChar = "";
        export.Occup.SelectChar = "";
        export.SpFn.SelectChar = "";
        export.SpLn.SelectChar = "";
        export.SpMi.SelectChar = "";
        export.WorkPh.SelectChar = "";
        UseSiRetrieveApLocCompareInfo();

        if (!IsExitState("CSENET_AP_LOCATE_NF"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "UPDATE":
        local.CsePerson.Assign(export.CsePerson);
        local.New1.Assign(export.CsePersonLicense);

        switch(AsChar(import.Occup.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.InterstateApLocate.Occupation))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.Occupation =
                export.InterstateApLocate.Occupation ?? "";
            }
            else
            {
              var field1 = GetField(export.Occup, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.InterstateApLocate, "occupation");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Occup, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.WorkPh.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.InterstateApLocate.WorkPhoneNumber.
              GetValueOrDefault() > 0 || export
              .InterstateApLocate.WorkAreaCode.GetValueOrDefault() > 0)
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.WorkPhoneAreaCode =
                export.InterstateApLocate.WorkAreaCode.GetValueOrDefault();
              local.CsePerson.WorkPhone =
                export.InterstateApLocate.WorkPhoneNumber.GetValueOrDefault();
            }
            else
            {
              var field1 = GetField(export.WorkPh, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.InterstateApLocate, "workAreaCode");

              field2.Color = "red";
              field2.Protected = true;

              var field3 =
                GetField(export.InterstateApLocate, "workPhoneNumber");

              field3.Color = "red";
              field3.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.WorkPh, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.HomePh.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (export.InterstateApLocate.HomePhoneNumber.
              GetValueOrDefault() > 0 || export
              .InterstateApLocate.HomeAreaCode.GetValueOrDefault() > 0)
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.HomePhoneAreaCode =
                export.InterstateApLocate.HomeAreaCode.GetValueOrDefault();
              local.CsePerson.HomePhone =
                export.InterstateApLocate.HomePhoneNumber.GetValueOrDefault();
            }
            else
            {
              var field1 = GetField(export.HomePh, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.InterstateApLocate, "homeAreaCode");

              field2.Color = "red";
              field2.Protected = true;

              var field3 =
                GetField(export.InterstateApLocate, "homePhoneNumber");

              field3.Color = "red";
              field3.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.HomePh, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.SpMi.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.InterstateApLocate.CurrentSpouseMiddleName))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.CurrentSpouseMi =
                import.InterstateApLocate.CurrentSpouseMiddleName ?? "";
            }
            else
            {
              var field1 = GetField(export.SpMi, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateApLocate, "currentSpouseMiddleName");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.SpMi, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.SpLn.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.InterstateApLocate.CurrentSpouseLastName))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.CurrentSpouseLastName =
                import.InterstateApLocate.CurrentSpouseLastName ?? "";
            }
            else
            {
              var field1 = GetField(export.SpLn, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateApLocate, "currentSpouseLastName");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.SpLn, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.SpFn.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.InterstateApLocate.CurrentSpouseFirstName))
            {
              local.UpdCsePerson.Flag = "Y";
              local.CsePerson.CurrentSpouseFirstName =
                import.InterstateApLocate.CurrentSpouseFirstName ?? "";
            }
            else
            {
              var field1 = GetField(export.SpFn, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateApLocate, "currentSpouseFirstName");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.SpFn, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.DlSt.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (!IsEmpty(export.InterstateApLocate.DriversLicState))
            {
              local.UpdCsePersonLicense.Flag = "Y";
              local.New1.IssuingState =
                export.InterstateApLocate.DriversLicState ?? "";
            }
            else
            {
              var field1 = GetField(export.DlSt, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateApLocate, "driversLicState");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.DlSt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        switch(AsChar(import.Dl.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            if (AsChar(import.DlSt.SelectChar) != 'S')
            {
              var field1 = GetField(export.DlSt, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }

            if (!IsEmpty(export.InterstateApLocate.DriversLicenseNum))
            {
              local.UpdCsePersonLicense.Flag = "Y";
              local.New1.Number =
                export.InterstateApLocate.DriversLicenseNum ?? "";
            }
            else
            {
              var field1 = GetField(export.Dl, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateApLocate, "driversLicenseNum");

              field2.Color = "red";
              field2.Protected = true;

              ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";
            }

            break;
          default:
            var field = GetField(export.Dl, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(local.UpdCsePerson.Flag) == 'Y' || AsChar
          (local.UpdCsePersonLicense.Flag) == 'Y')
        {
          // *********************************************
          // Process the updates
          // *********************************************
          UseSiProcessComparisonData();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Dl.SelectChar = local.Dl.SelectChar;
            export.DlSt.SelectChar = local.DlSt.SelectChar;
            export.HomePh.SelectChar = local.HomePh.SelectChar;
            export.Occup.SelectChar = local.Occup.SelectChar;
            export.SpFn.SelectChar = local.SpFn.SelectChar;
            export.SpLn.SelectChar = local.SpLn.SelectChar;
            export.SpMi.SelectChar = local.SpMi.SelectChar;
            export.WorkPh.SelectChar = local.WorkPh.SelectChar;
            export.CsePerson.Assign(local.CsePerson);
            export.CsePersonLicense.Assign(local.New1);
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }
        else
        {
          var field = GetField(export.Dl, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "SCNX":
        if (import.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApCurrentInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_CURRENT";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
          }
        }
        else if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "NO_MORE_REFERRAL_SCREENS_FOUND";
        }

        break;
      case "IAPH":
        if (export.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
        {
          UseSiCheckApCurrHist();

          if (AsChar(local.ApHistoryInd.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
          }
          else
          {
            ExitState = "CSENET_DATA_DOES_NOT_EXIST";
          }
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "ISUP":
        ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";

        break;
      case "IMIS":
        ExitState = "ECO_LNK_TO_CSE_MISC";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
    target.KsCaseId = source.KsCaseId;
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

  private void UseSiCheckApCurrHist()
  {
    var useImport = new SiCheckApCurrHist.Import();
    var useExport = new SiCheckApCurrHist.Export();

    MoveInterstateCase1(export.InterstateCase, useImport.InterstateCase);

    Call(SiCheckApCurrHist.Execute, useImport, useExport);

    local.ApHistoryInd.Flag = useExport.ApHistoryInd.Flag;
    local.ApCurrentInd.Flag = useExport.ApCurrentInd.Flag;
  }

  private void UseSiProcessComparisonData()
  {
    var useImport = new SiProcessComparisonData.Import();
    var useExport = new SiProcessComparisonData.Export();

    useImport.New1.Assign(local.New1);
    useImport.CsePerson.Assign(local.CsePerson);
    useImport.UpdCsePersnLicenseInd.Flag = local.UpdCsePersonLicense.Flag;
    useImport.UpdCsePersonInd.Flag = local.UpdCsePerson.Flag;
    useImport.Old.Assign(export.CsePersonLicense);

    Call(SiProcessComparisonData.Execute, useImport, useExport);
  }

  private void UseSiRetrieveApLocCompareInfo()
  {
    var useImport = new SiRetrieveApLocCompareInfo.Import();
    var useExport = new SiRetrieveApLocCompareInfo.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveInterstateCase2(import.InterstateCase, useImport.InterstateCase);

    Call(SiRetrieveApLocCompareInfo.Execute, useImport, useExport);

    export.InterstateApLocate.Assign(useExport.InterstateApLocate);
    export.CsePerson.Assign(useExport.CsePerson);
    export.CsePersonLicense.Assign(useExport.CsePersonLicense);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A PartListGroup group.</summary>
    [Serializable]
    public class PartListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePersonsWorkSet g;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of Occup.
    /// </summary>
    [JsonPropertyName("occup")]
    public Common Occup
    {
      get => occup ??= new();
      set => occup = value;
    }

    /// <summary>
    /// A value of WorkPh.
    /// </summary>
    [JsonPropertyName("workPh")]
    public Common WorkPh
    {
      get => workPh ??= new();
      set => workPh = value;
    }

    /// <summary>
    /// A value of HomePh.
    /// </summary>
    [JsonPropertyName("homePh")]
    public Common HomePh
    {
      get => homePh ??= new();
      set => homePh = value;
    }

    /// <summary>
    /// A value of SpMi.
    /// </summary>
    [JsonPropertyName("spMi")]
    public Common SpMi
    {
      get => spMi ??= new();
      set => spMi = value;
    }

    /// <summary>
    /// A value of SpLn.
    /// </summary>
    [JsonPropertyName("spLn")]
    public Common SpLn
    {
      get => spLn ??= new();
      set => spLn = value;
    }

    /// <summary>
    /// A value of SpFn.
    /// </summary>
    [JsonPropertyName("spFn")]
    public Common SpFn
    {
      get => spFn ??= new();
      set => spFn = value;
    }

    /// <summary>
    /// A value of DlSt.
    /// </summary>
    [JsonPropertyName("dlSt")]
    public Common DlSt
    {
      get => dlSt ??= new();
      set => dlSt = value;
    }

    /// <summary>
    /// A value of Dl.
    /// </summary>
    [JsonPropertyName("dl")]
    public Common Dl
    {
      get => dl ??= new();
      set => dl = value;
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
    /// Gets a value of PartList.
    /// </summary>
    [JsonIgnore]
    public Array<PartListGroup> PartList => partList ??= new(
      PartListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PartList for json serialization.
    /// </summary>
    [JsonPropertyName("partList")]
    [Computed]
    public IList<PartListGroup> PartList_Json
    {
      get => partList;
      set => PartList.Assign(value);
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

    private InterstateCase interstateCase;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
    private InterstateApLocate interstateApLocate;
    private Common occup;
    private Common workPh;
    private Common homePh;
    private Common spMi;
    private Common spLn;
    private Common spFn;
    private Common dlSt;
    private Common dl;
    private Standard standard;
    private Array<PartListGroup> partList;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PartListGroup group.</summary>
    [Serializable]
    public class PartListGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePersonsWorkSet g;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of Occup.
    /// </summary>
    [JsonPropertyName("occup")]
    public Common Occup
    {
      get => occup ??= new();
      set => occup = value;
    }

    /// <summary>
    /// A value of WorkPh.
    /// </summary>
    [JsonPropertyName("workPh")]
    public Common WorkPh
    {
      get => workPh ??= new();
      set => workPh = value;
    }

    /// <summary>
    /// A value of HomePh.
    /// </summary>
    [JsonPropertyName("homePh")]
    public Common HomePh
    {
      get => homePh ??= new();
      set => homePh = value;
    }

    /// <summary>
    /// A value of SpMi.
    /// </summary>
    [JsonPropertyName("spMi")]
    public Common SpMi
    {
      get => spMi ??= new();
      set => spMi = value;
    }

    /// <summary>
    /// A value of SpLn.
    /// </summary>
    [JsonPropertyName("spLn")]
    public Common SpLn
    {
      get => spLn ??= new();
      set => spLn = value;
    }

    /// <summary>
    /// A value of SpFn.
    /// </summary>
    [JsonPropertyName("spFn")]
    public Common SpFn
    {
      get => spFn ??= new();
      set => spFn = value;
    }

    /// <summary>
    /// A value of DlSt.
    /// </summary>
    [JsonPropertyName("dlSt")]
    public Common DlSt
    {
      get => dlSt ??= new();
      set => dlSt = value;
    }

    /// <summary>
    /// A value of Dl.
    /// </summary>
    [JsonPropertyName("dl")]
    public Common Dl
    {
      get => dl ??= new();
      set => dl = value;
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
    /// Gets a value of PartList.
    /// </summary>
    [JsonIgnore]
    public Array<PartListGroup> PartList => partList ??= new(
      PartListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PartList for json serialization.
    /// </summary>
    [JsonPropertyName("partList")]
    [Computed]
    public IList<PartListGroup> PartList_Json
    {
      get => partList;
      set => PartList.Assign(value);
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

    private InterstateCase interstateCase;
    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
    private InterstateApLocate interstateApLocate;
    private Common occup;
    private Common workPh;
    private Common homePh;
    private Common spMi;
    private Common spLn;
    private Common spFn;
    private Common dlSt;
    private Common dl;
    private Standard standard;
    private Array<PartListGroup> partList;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CsePersonLicense New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Occup.
    /// </summary>
    [JsonPropertyName("occup")]
    public Common Occup
    {
      get => occup ??= new();
      set => occup = value;
    }

    /// <summary>
    /// A value of WorkPh.
    /// </summary>
    [JsonPropertyName("workPh")]
    public Common WorkPh
    {
      get => workPh ??= new();
      set => workPh = value;
    }

    /// <summary>
    /// A value of HomePh.
    /// </summary>
    [JsonPropertyName("homePh")]
    public Common HomePh
    {
      get => homePh ??= new();
      set => homePh = value;
    }

    /// <summary>
    /// A value of SpMi.
    /// </summary>
    [JsonPropertyName("spMi")]
    public Common SpMi
    {
      get => spMi ??= new();
      set => spMi = value;
    }

    /// <summary>
    /// A value of SpLn.
    /// </summary>
    [JsonPropertyName("spLn")]
    public Common SpLn
    {
      get => spLn ??= new();
      set => spLn = value;
    }

    /// <summary>
    /// A value of SpFn.
    /// </summary>
    [JsonPropertyName("spFn")]
    public Common SpFn
    {
      get => spFn ??= new();
      set => spFn = value;
    }

    /// <summary>
    /// A value of DlSt.
    /// </summary>
    [JsonPropertyName("dlSt")]
    public Common DlSt
    {
      get => dlSt ??= new();
      set => dlSt = value;
    }

    /// <summary>
    /// A value of Dl.
    /// </summary>
    [JsonPropertyName("dl")]
    public Common Dl
    {
      get => dl ??= new();
      set => dl = value;
    }

    /// <summary>
    /// A value of ZdelLocalCreateCsePersonLic.
    /// </summary>
    [JsonPropertyName("zdelLocalCreateCsePersonLic")]
    public Common ZdelLocalCreateCsePersonLic
    {
      get => zdelLocalCreateCsePersonLic ??= new();
      set => zdelLocalCreateCsePersonLic = value;
    }

    /// <summary>
    /// A value of UpdCsePersonLicense.
    /// </summary>
    [JsonPropertyName("updCsePersonLicense")]
    public Common UpdCsePersonLicense
    {
      get => updCsePersonLicense ??= new();
      set => updCsePersonLicense = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of UpdCsePerson.
    /// </summary>
    [JsonPropertyName("updCsePerson")]
    public Common UpdCsePerson
    {
      get => updCsePerson ??= new();
      set => updCsePerson = value;
    }

    /// <summary>
    /// A value of ApHistoryInd.
    /// </summary>
    [JsonPropertyName("apHistoryInd")]
    public Common ApHistoryInd
    {
      get => apHistoryInd ??= new();
      set => apHistoryInd = value;
    }

    /// <summary>
    /// A value of ApCurrentInd.
    /// </summary>
    [JsonPropertyName("apCurrentInd")]
    public Common ApCurrentInd
    {
      get => apCurrentInd ??= new();
      set => apCurrentInd = value;
    }

    private CsePersonLicense new1;
    private CsePerson csePerson;
    private Common occup;
    private Common workPh;
    private Common homePh;
    private Common spMi;
    private Common spLn;
    private Common spFn;
    private Common dlSt;
    private Common dl;
    private Common zdelLocalCreateCsePersonLic;
    private Common updCsePersonLicense;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common updCsePerson;
    private Common apHistoryInd;
    private Common apCurrentInd;
  }
#endregion
}

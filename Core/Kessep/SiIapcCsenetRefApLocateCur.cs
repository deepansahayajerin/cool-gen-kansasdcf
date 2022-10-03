// Program: SI_IAPC_CSENET_REF_AP_LOCATE_CUR, ID: 372511284, model: 746.
// Short name: SWEIAPCP
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
/// A program: SI_IAPC_CSENET_REF_AP_LOCATE_CUR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIapcCsenetRefApLocateCur: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IAPC_CSENET_REF_AP_LOCATE_CUR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIapcCsenetRefApLocateCur(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIapcCsenetRefApLocateCur.
  /// </summary>
  public SiIapcCsenetRefApLocateCur(IContext context, Import import,
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
    //         M A I N T E N A N C E   l O G
    // Date	  Developer		Description
    // 04/20/95  Sherri Newman		Initial Dev.
    // 01/25/96  Randy Haas		Retro Fit
    // 11/04/96  G. Lofton		Add new security and removed
    // 				old.
    // 04/02/00 C. Scroggins           Added view and matching for family 
    // violence.
    // ------------------------------------------------------------
    // *********************************************
    // *  This PRAD is for the CSE - Interstate AP *
    // *  Current Screen which will display CSENet *
    // *  current AP information.                  *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    MoveInterstateApIdentification(import.InterstateApIdentification,
      export.InterstateApIdentification);
    export.InterstateApLocate.Assign(import.InterstateApLocate);
    export.InterstateCase.Assign(import.InterstateCase);
    export.Ap.FormattedName = import.Ap.FormattedName;
    export.Spouse.FormattedName = import.Spouse.FormattedName;
    MoveStandard(import.Standard, export.Standard);
    MoveOffice(import.Office, export.Office);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);

    if (!import.Employment.IsEmpty)
    {
      export.Employment.Index = 0;
      export.Employment.Clear();

      for(import.Employment.Index = 0; import.Employment.Index < import
        .Employment.Count; ++import.Employment.Index)
      {
        if (export.Employment.IsFull)
        {
          break;
        }

        export.Employment.Update.InterstateApLocate.Assign(
          import.Employment.Item.InterstateApLocate);
        export.Employment.Next();
      }
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
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
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (!IsEmpty(export.InterstateCase.KsCaseId))
    {
      local.Case1.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
    }

    switch(TrimEnd(global.Command))
    {
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
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        UseSiReadApLocateCurrent();

        if (StringToNumber(export.InterstateApLocate.ResidentialZipCode5) == 0)
        {
          export.InterstateApLocate.ResidentialZipCode5 = "";
        }

        if (StringToNumber(export.InterstateApLocate.ResidentialZipCode4) == 0)
        {
          export.InterstateApLocate.ResidentialZipCode4 = "";
        }

        if (StringToNumber(export.InterstateApLocate.MailingZipCode5) == 0)
        {
          export.InterstateApLocate.MailingZipCode5 = "";
        }

        if (StringToNumber(export.InterstateApLocate.MailingZipCode4) == 0)
        {
          export.InterstateApLocate.MailingZipCode4 = "";
        }

        if (StringToNumber(export.InterstateApIdentification.Ssn) == 0)
        {
          export.InterstateApIdentification.Ssn = "";
        }

        break;
      case "SCNX":
        UseSiCheckApCurrHist();

        if (AsChar(export.ApHistoryInd.Flag) == 'Y')
        {
          ExitState = "ECO_LNK_TO_CSE_AP_HISTORY";
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
      case "ISUP":
        if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_SUPPORT_ORDER";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "IMIS":
        if (import.InterstateCase.InformationInd.GetValueOrDefault() > 0)
        {
          ExitState = "ECO_LNK_TO_CSE_MISC";
        }
        else
        {
          ExitState = "CSENET_DATA_DOES_NOT_EXIST";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveEmployment(SiReadApLocateCurrent.Export.
    EmploymentGroup source, Export.EmploymentGroup target)
  {
    target.InterstateApLocate.Assign(source.InterstateApLocate);
  }

  private static void MoveInterstateApIdentification(
    InterstateApIdentification source, InterstateApIdentification target)
  {
    target.Ssn = source.Ssn;
    target.DateOfBirth = source.DateOfBirth;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.MenuOption = source.MenuOption;
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

    useImport.Case1.Number = local.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCheckApCurrHist()
  {
    var useImport = new SiCheckApCurrHist.Import();
    var useExport = new SiCheckApCurrHist.Export();

    MoveInterstateCase(export.InterstateCase, useImport.InterstateCase);

    Call(SiCheckApCurrHist.Execute, useImport, useExport);

    export.ApHistoryInd.Flag = useExport.ApHistoryInd.Flag;
  }

  private void UseSiReadApLocateCurrent()
  {
    var useImport = new SiReadApLocateCurrent.Import();
    var useExport = new SiReadApLocateCurrent.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadApLocateCurrent.Execute, useImport, useExport);

    export.Spouse.FormattedName = useExport.Spouse.FormattedName;
    export.Ap.FormattedName = useExport.Ap.FormattedName;
    export.InterstateApLocate.Assign(useExport.InterstateApLocate);
    MoveInterstateApIdentification(useExport.InterstateApIdentification,
      export.InterstateApIdentification);
    useExport.Employment.CopyTo(export.Employment, MoveEmployment);
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
    /// <summary>A EmploymentGroup group.</summary>
    [Serializable]
    public class EmploymentGroup
    {
      /// <summary>
      /// A value of InterstateApLocate.
      /// </summary>
      [JsonPropertyName("interstateApLocate")]
      public InterstateApLocate InterstateApLocate
      {
        get => interstateApLocate ??= new();
        set => interstateApLocate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private InterstateApLocate interstateApLocate;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Spouse.
    /// </summary>
    [JsonPropertyName("spouse")]
    public CsePersonsWorkSet Spouse
    {
      get => spouse ??= new();
      set => spouse = value;
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
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// Gets a value of Employment.
    /// </summary>
    [JsonIgnore]
    public Array<EmploymentGroup> Employment => employment ??= new(
      EmploymentGroup.Capacity);

    /// <summary>
    /// Gets a value of Employment for json serialization.
    /// </summary>
    [JsonPropertyName("employment")]
    [Computed]
    public IList<EmploymentGroup> Employment_Json
    {
      get => employment;
      set => Employment.Assign(value);
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet spouse;
    private Common apHistoryInd;
    private Common apCurrentInd;
    private InterstateCase interstateCase;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<EmploymentGroup> employment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EmploymentGroup group.</summary>
    [Serializable]
    public class EmploymentGroup
    {
      /// <summary>
      /// A value of InterstateApLocate.
      /// </summary>
      [JsonPropertyName("interstateApLocate")]
      public InterstateApLocate InterstateApLocate
      {
        get => interstateApLocate ??= new();
        set => interstateApLocate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private InterstateApLocate interstateApLocate;
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

    /// <summary>
    /// A value of Spouse.
    /// </summary>
    [JsonPropertyName("spouse")]
    public CsePersonsWorkSet Spouse
    {
      get => spouse ??= new();
      set => spouse = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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
    /// Gets a value of Employment.
    /// </summary>
    [JsonIgnore]
    public Array<EmploymentGroup> Employment => employment ??= new(
      EmploymentGroup.Capacity);

    /// <summary>
    /// Gets a value of Employment for json serialization.
    /// </summary>
    [JsonPropertyName("employment")]
    [Computed]
    public IList<EmploymentGroup> Employment_Json
    {
      get => employment;
      set => Employment.Assign(value);
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    private Common apHistoryInd;
    private Common apCurrentInd;
    private CsePersonsWorkSet spouse;
    private CsePersonsWorkSet ap;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<EmploymentGroup> employment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }
#endregion
}

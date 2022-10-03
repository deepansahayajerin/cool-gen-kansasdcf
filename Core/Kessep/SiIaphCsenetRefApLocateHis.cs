// Program: SI_IAPH_CSENET_REF_AP_LOCATE_HIS, ID: 372512437, model: 746.
// Short name: SWEIAPHP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_IAPH_CSENET_REF_AP_LOCATE_HIS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiIaphCsenetRefApLocateHis: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IAPH_CSENET_REF_AP_LOCATE_HIS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIaphCsenetRefApLocateHis(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIaphCsenetRefApLocateHis.
  /// </summary>
  public SiIaphCsenetRefApLocateHis(IContext context, Import import,
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
    // 06/12/95  Sherri Newman		Initial Dev.
    // 02/1/96   Randy Haas		Retro Fit
    // 11/04/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 04/03/00 C. Scroggins           Added view and view
    //                                 
    // matching for family violence.
    // ------------------------------------------------------------
    // ---------------------------------------------
    //    This PRAD is for the CSE - Interstate AP
    //    History Screen which will display CSENet
    //    history AP information.
    // ---------------------------------------------
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
    export.Alias1.FormattedName = import.Alias1.FormattedName;
    export.Alias2.FormattedName = import.Alias2.FormattedName;
    export.Alias3.FormattedName = import.Alias3.FormattedName;
    export.Ap.FormattedName = import.Ap.FormattedName;
    export.ServiceProvider.Assign(import.ServiceProvider);
    MoveOffice(import.Office, export.Office);
    export.OfficeServiceProvider.Assign(import.OfficeServiceProvider);
    export.ServiceProviderAddress.Assign(import.ServiceProviderAddress);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

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
      else
      {
        return;
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
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        UseSiReadApLocateHistory();

        if (StringToNumber(export.InterstateApLocate.LastResZipCode5) == 0)
        {
          export.InterstateApLocate.LastResZipCode5 = "";
        }

        if (StringToNumber(export.InterstateApLocate.LastResZipCode4) == 0)
        {
          export.InterstateApLocate.LastResZipCode4 = "";
        }

        if (StringToNumber(export.InterstateApLocate.LastMailZipCode5) == 0)
        {
          export.InterstateApLocate.LastMailZipCode5 = "";
        }

        if (StringToNumber(export.InterstateApLocate.LastMailZipCode4) == 0)
        {
          export.InterstateApLocate.LastMailZipCode4 = "";
        }

        if (StringToNumber(export.InterstateApLocate.LastEmployerZipCode5) == 0)
        {
          export.InterstateApLocate.LastEmployerZipCode5 = "";
        }

        if (StringToNumber(export.InterstateApLocate.LastEmployerZipCode4) == 0)
        {
          export.InterstateApLocate.LastEmployerZipCode4 = "";
        }

        if (Equal(export.InterstateApLocate.LastResAddressDate,
          new DateTime(2099, 12, 31)))
        {
          export.InterstateApLocate.LastResAddressDate = null;
        }

        break;
      case "SCNX":
        if (import.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
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
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private void UseSiReadApLocateHistory()
  {
    var useImport = new SiReadApLocateHistory.Import();
    var useExport = new SiReadApLocateHistory.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiReadApLocateHistory.Execute, useImport, useExport);

    export.Ap.FormattedName = useExport.Ap.FormattedName;
    export.Alias1.FormattedName = useExport.Alias1.FormattedName;
    export.Alias2.FormattedName = useExport.Alias2.FormattedName;
    export.Alias3.FormattedName = useExport.Alias3.FormattedName;
    MoveInterstateApIdentification(useExport.InterstateApIdentification,
      export.InterstateApIdentification);
    export.InterstateApLocate.Assign(useExport.InterstateApLocate);
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
    /// A value of Alias1.
    /// </summary>
    [JsonPropertyName("alias1")]
    public CsePersonsWorkSet Alias1
    {
      get => alias1 ??= new();
      set => alias1 = value;
    }

    /// <summary>
    /// A value of Alias2.
    /// </summary>
    [JsonPropertyName("alias2")]
    public CsePersonsWorkSet Alias2
    {
      get => alias2 ??= new();
      set => alias2 = value;
    }

    /// <summary>
    /// A value of Alias3.
    /// </summary>
    [JsonPropertyName("alias3")]
    public CsePersonsWorkSet Alias3
    {
      get => alias3 ??= new();
      set => alias3 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    private CsePersonsWorkSet alias1;
    private CsePersonsWorkSet alias2;
    private CsePersonsWorkSet alias3;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProviderAddress serviceProviderAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Alias1.
    /// </summary>
    [JsonPropertyName("alias1")]
    public CsePersonsWorkSet Alias1
    {
      get => alias1 ??= new();
      set => alias1 = value;
    }

    /// <summary>
    /// A value of Alias2.
    /// </summary>
    [JsonPropertyName("alias2")]
    public CsePersonsWorkSet Alias2
    {
      get => alias2 ??= new();
      set => alias2 = value;
    }

    /// <summary>
    /// A value of Alias3.
    /// </summary>
    [JsonPropertyName("alias3")]
    public CsePersonsWorkSet Alias3
    {
      get => alias3 ??= new();
      set => alias3 = value;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    private CsePersonsWorkSet alias1;
    private CsePersonsWorkSet alias2;
    private CsePersonsWorkSet alias3;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
    private Standard standard;
    private NextTranInfo hidden;
    private ServiceProvider serviceProvider;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
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

// Program: OE_CAB_SET_MNEMONICS, ID: 371794507, model: 746.
// Short name: SWE00949
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_SET_MNEMONICS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
///    This action block sets all the codes that are used for various attributes
/// in the OBLGESTB business system.
/// </para>
/// </summary>
[Serializable]
public partial class OeCabSetMnemonics: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_SET_MNEMONICS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabSetMnemonics(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabSetMnemonics.
  /// </summary>
  public OeCabSetMnemonics(IContext context, Import import, Export export):
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
    // This action block sets the codes that are
    // used by the attributes in the OBGLESTB BSI.
    // ---------------------------------------------
    export.MailingAddressType.AddressType = "M";
    export.ResidenceAddressType.AddressType = "R";
    UseCabSetMaximumDiscontinueDate();
    export.MaxDate.ExpirationDate = local.DateWorkArea.Date;
    export.MilitaryBranch.CodeName = "MILITARY BRANCH";
    export.MilitaryRank.CodeName = "MILITARY RANK";
    export.MilitaryDutyStatus.CodeName = "MILITARY DUTY STATUS";
    export.HealthInsurance.Classification = "H";
    export.State.CodeName = "STATE CODE";
    export.Country.CodeName = "COUNTRY CODE";
    export.TribalAgency.CodeName = "TRIBAL IV-D AGENCIES";
    export.CsenetActionType.CodeName = "INTERSTATE ACTION";
    export.CsenetFunctionalType.CodeName = "INTERSTATE FUNCTION";
    export.CsenetActionReason.CodeName = "INTERSTATE REASON";
    export.CsenetOgCaseReason.CodeName = "OUTGOING CASE REASON";
    export.CsenetOgTranReason.CodeName = "OUTGOING TRANSACTION REASON";
    export.CsenetCaseClosure.CodeName = "CASE CLOSURE REASON";
    export.CsenetProgramType.CodeName = "INTERSTATE CASE PROGRAM";
    export.InterstateReqAttachment.CodeName = "INTERSTATE ATTACHMENTS";
    export.AddressSource.CodeName = "ADDRESS SOURCE";
    export.AddressEndCode.CodeName = "ADDRESS END";
    export.AddressVerifiedCode.CodeName = "ADDRESS RETURN";
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TribalAgency.
    /// </summary>
    [JsonPropertyName("tribalAgency")]
    public Code TribalAgency
    {
      get => tribalAgency ??= new();
      set => tribalAgency = value;
    }

    /// <summary>
    /// A value of CsenetProgramType.
    /// </summary>
    [JsonPropertyName("csenetProgramType")]
    public Code CsenetProgramType
    {
      get => csenetProgramType ??= new();
      set => csenetProgramType = value;
    }

    /// <summary>
    /// A value of CsenetOgCaseReason.
    /// </summary>
    [JsonPropertyName("csenetOgCaseReason")]
    public Code CsenetOgCaseReason
    {
      get => csenetOgCaseReason ??= new();
      set => csenetOgCaseReason = value;
    }

    /// <summary>
    /// A value of CsenetOgTranReason.
    /// </summary>
    [JsonPropertyName("csenetOgTranReason")]
    public Code CsenetOgTranReason
    {
      get => csenetOgTranReason ??= new();
      set => csenetOgTranReason = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of AddressSource.
    /// </summary>
    [JsonPropertyName("addressSource")]
    public Code AddressSource
    {
      get => addressSource ??= new();
      set => addressSource = value;
    }

    /// <summary>
    /// A value of AddressVerifiedCode.
    /// </summary>
    [JsonPropertyName("addressVerifiedCode")]
    public Code AddressVerifiedCode
    {
      get => addressVerifiedCode ??= new();
      set => addressVerifiedCode = value;
    }

    /// <summary>
    /// A value of AddressEndCode.
    /// </summary>
    [JsonPropertyName("addressEndCode")]
    public Code AddressEndCode
    {
      get => addressEndCode ??= new();
      set => addressEndCode = value;
    }

    /// <summary>
    /// A value of InterstateReqAttachment.
    /// </summary>
    [JsonPropertyName("interstateReqAttachment")]
    public Code InterstateReqAttachment
    {
      get => interstateReqAttachment ??= new();
      set => interstateReqAttachment = value;
    }

    /// <summary>
    /// A value of CsenetCaseClosure.
    /// </summary>
    [JsonPropertyName("csenetCaseClosure")]
    public Code CsenetCaseClosure
    {
      get => csenetCaseClosure ??= new();
      set => csenetCaseClosure = value;
    }

    /// <summary>
    /// A value of CsenetActionReason.
    /// </summary>
    [JsonPropertyName("csenetActionReason")]
    public Code CsenetActionReason
    {
      get => csenetActionReason ??= new();
      set => csenetActionReason = value;
    }

    /// <summary>
    /// A value of CsenetActionType.
    /// </summary>
    [JsonPropertyName("csenetActionType")]
    public Code CsenetActionType
    {
      get => csenetActionType ??= new();
      set => csenetActionType = value;
    }

    /// <summary>
    /// A value of CsenetFunctionalType.
    /// </summary>
    [JsonPropertyName("csenetFunctionalType")]
    public Code CsenetFunctionalType
    {
      get => csenetFunctionalType ??= new();
      set => csenetFunctionalType = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of HealthInsurance.
    /// </summary>
    [JsonPropertyName("healthInsurance")]
    public ObligationType HealthInsurance
    {
      get => healthInsurance ??= new();
      set => healthInsurance = value;
    }

    /// <summary>
    /// A value of MilitaryDutyStatus.
    /// </summary>
    [JsonPropertyName("militaryDutyStatus")]
    public Code MilitaryDutyStatus
    {
      get => militaryDutyStatus ??= new();
      set => militaryDutyStatus = value;
    }

    /// <summary>
    /// A value of MilitaryRank.
    /// </summary>
    [JsonPropertyName("militaryRank")]
    public Code MilitaryRank
    {
      get => militaryRank ??= new();
      set => militaryRank = value;
    }

    /// <summary>
    /// A value of MilitaryBranch.
    /// </summary>
    [JsonPropertyName("militaryBranch")]
    public Code MilitaryBranch
    {
      get => militaryBranch ??= new();
      set => militaryBranch = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ResidenceAddressType.
    /// </summary>
    [JsonPropertyName("residenceAddressType")]
    public HealthInsuranceCompanyAddress ResidenceAddressType
    {
      get => residenceAddressType ??= new();
      set => residenceAddressType = value;
    }

    /// <summary>
    /// A value of MailingAddressType.
    /// </summary>
    [JsonPropertyName("mailingAddressType")]
    public HealthInsuranceCompanyAddress MailingAddressType
    {
      get => mailingAddressType ??= new();
      set => mailingAddressType = value;
    }

    private Code tribalAgency;
    private Code csenetProgramType;
    private Code csenetOgCaseReason;
    private Code csenetOgTranReason;
    private Code country;
    private Code addressSource;
    private Code addressVerifiedCode;
    private Code addressEndCode;
    private Code interstateReqAttachment;
    private Code csenetCaseClosure;
    private Code csenetActionReason;
    private Code csenetActionType;
    private Code csenetFunctionalType;
    private Code state;
    private ObligationType healthInsurance;
    private Code militaryDutyStatus;
    private Code militaryRank;
    private Code militaryBranch;
    private Code maxDate;
    private HealthInsuranceCompanyAddress residenceAddressType;
    private HealthInsuranceCompanyAddress mailingAddressType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }
#endregion
}

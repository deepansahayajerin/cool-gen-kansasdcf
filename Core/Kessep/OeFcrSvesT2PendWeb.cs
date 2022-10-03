// Program: OE_FCR_SVES_T2_PEND_WEB, ID: 945076820, model: 746.
// Short name: SWCSV02P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T2_PEND_WEB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class OeFcrSvesT2PendWeb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T2_PEND_WEB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT2PendWeb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT2PendWeb.
  /// </summary>
  public OeFcrSvesT2PendWeb(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // * ----------  -----------------  ---------   -----------------------
    // * 07/08/11    LSS                CQ5577      Initial Coding.
    // *
    // * 12/13/13    LSS                CQ42137     Change security access to 
    // the web page back to the
    // *
    // 
    // original security to allow only the assigned
    // case worker
    // *
    // 
    // and their supervisor access to view FPLS
    // data per business
    // *
    // 
    // requirement dated 11/27/13 to be in
    // compliance with
    // *
    // 
    // the FPLS Security Agreement.
    // 03/29/2018    JLH    CQ61705     Remove edit that only allows case worker
    // and supervisor to see SVES information.
    // *************************************************************
    // Fields needing formatted (dates, ssn, phone) are being exported as 
    // formatted work views.
    ExitState = "ACO_NN0000_ALL_OK";
    global.Command = "DISPLAY";
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;

        return;
      }
    }

    // Security code change per the following business rule change:
    // The original security feature to the FPLS screen allowed only the 
    // assigned case worker and their supervisor
    // the ability to view data via the FPLS screen.  Effective 9/13/2011, the 
    // additional layer of security is to
    // be removed, per central office decision.  Any profile deemed appropriate 
    // will have the ability to view FPLS data,
    // the user does not have to be assigned to the case any longer.
    // *The business rule for SVES is to have the same security as FPLS
    // The following is the profile authorization level of security that 
    // replaces the case assignment level of
    // security in the above commented code.   Per central office business 
    // decision 9/13/11.
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;

      return;
    }

    // END - CQ42137 Security access
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeFcrSvesPersonRecord();
    }

    // Gets the common fields information for all SVES Types as General 
    // Information
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeFcrSvesT2Pend();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;
    }
  }

  private static void MoveFcrSvesGenInfo1(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.UserField = source.UserField;
    target.FipsCountyCode = source.FipsCountyCode;
    target.ParticipantType = source.ParticipantType;
  }

  private static void MoveFcrSvesGenInfo2(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
    target.TransmitterStateTerritoryCode = source.TransmitterStateTerritoryCode;
    target.SubmittedFirstName = source.SubmittedFirstName;
    target.SubmittedMiddleName = source.SubmittedMiddleName;
    target.SubmittedLastName = source.SubmittedLastName;
  }

  private static void MoveSvesType(OeFcrSvesPersonRecord.Export.
    SvesTypeGroup source, Export.SvesTypeGroup target)
  {
    target.Gagency.LocateSourceResponseAgencyCo =
      source.Gtype.LocateSourceResponseAgencyCo;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeFcrSvesPersonRecord()
  {
    var useImport = new OeFcrSvesPersonRecord.Import();
    var useExport = new OeFcrSvesPersonRecord.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesPersonRecord.Execute, useImport, useExport);

    export.FormattedReqDate.Text10 = useExport.FormattedReqDate.Text10;
    export.FormattedRespRecDate.Text10 = useExport.FormattedRespRecDate.Text10;
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    useExport.SvesType.CopyTo(export.SvesType, MoveSvesType);
    MoveFcrSvesGenInfo2(useExport.FcrSvesGenInfo, export.FcrSvesGenInfo);
  }

  private void UseOeFcrSvesT2Pend()
  {
    var useImport = new OeFcrSvesT2Pend.Import();
    var useExport = new OeFcrSvesT2Pend.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesT2Pend.Execute, useImport, useExport);

    export.FormattedT2ResponseDat.Text10 =
      useExport.FormattedT2ResponseDat.Text10;
    export.FormattedFcrRecDate.Text10 = useExport.FormattedFcrRecDate.Text10;
    export.FormattedOtherSsn.Text11 = useExport.FormattedOtherSsn.Text11;
    export.FormattedVerifiedSsn.Text11 = useExport.FormattedVerifiedSsn.Text11;
    export.FormattedPrimarySsn.Text11 = useExport.FormattedPrimarySsn.Text11;
    MoveFcrSvesGenInfo1(useExport.FcrSvesGenInfo, export.FcrSvesGenInfo);
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    export.Cl02.Assign(useExport.Cl02);
    export.Do03.Assign(useExport.Do03);
    export.FcrSvesTitleIiPend.Assign(useExport.FcrSvesTitleIiPend);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SvesTypeGroup group.</summary>
    [Serializable]
    public class SvesTypeGroup
    {
      /// <summary>
      /// A value of Gagency.
      /// </summary>
      [JsonPropertyName("gagency")]
      public FcrSvesGenInfo Gagency
      {
        get => gagency ??= new();
        set => gagency = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private FcrSvesGenInfo gagency;
    }

    /// <summary>
    /// A value of FormattedT2ResponseDat.
    /// </summary>
    [JsonPropertyName("formattedT2ResponseDat")]
    public WorkArea FormattedT2ResponseDat
    {
      get => formattedT2ResponseDat ??= new();
      set => formattedT2ResponseDat = value;
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
    /// A value of FormattedReqDate.
    /// </summary>
    [JsonPropertyName("formattedReqDate")]
    public WorkArea FormattedReqDate
    {
      get => formattedReqDate ??= new();
      set => formattedReqDate = value;
    }

    /// <summary>
    /// A value of FormattedRespRecDate.
    /// </summary>
    [JsonPropertyName("formattedRespRecDate")]
    public WorkArea FormattedRespRecDate
    {
      get => formattedRespRecDate ??= new();
      set => formattedRespRecDate = value;
    }

    /// <summary>
    /// A value of FormattedFcrRecDate.
    /// </summary>
    [JsonPropertyName("formattedFcrRecDate")]
    public WorkArea FormattedFcrRecDate
    {
      get => formattedFcrRecDate ??= new();
      set => formattedFcrRecDate = value;
    }

    /// <summary>
    /// A value of FormattedPrimarySsn.
    /// </summary>
    [JsonPropertyName("formattedPrimarySsn")]
    public WorkArea FormattedPrimarySsn
    {
      get => formattedPrimarySsn ??= new();
      set => formattedPrimarySsn = value;
    }

    /// <summary>
    /// A value of FormattedVerifiedSsn.
    /// </summary>
    [JsonPropertyName("formattedVerifiedSsn")]
    public WorkArea FormattedVerifiedSsn
    {
      get => formattedVerifiedSsn ??= new();
      set => formattedVerifiedSsn = value;
    }

    /// <summary>
    /// A value of FormattedOtherSsn.
    /// </summary>
    [JsonPropertyName("formattedOtherSsn")]
    public WorkArea FormattedOtherSsn
    {
      get => formattedOtherSsn ??= new();
      set => formattedOtherSsn = value;
    }

    /// <summary>
    /// A value of SvesErrorMsgs.
    /// </summary>
    [JsonPropertyName("svesErrorMsgs")]
    public WorkArea SvesErrorMsgs
    {
      get => svesErrorMsgs ??= new();
      set => svesErrorMsgs = value;
    }

    /// <summary>
    /// Gets a value of SvesType.
    /// </summary>
    [JsonIgnore]
    public Array<SvesTypeGroup> SvesType => svesType ??= new(
      SvesTypeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SvesType for json serialization.
    /// </summary>
    [JsonPropertyName("svesType")]
    [Computed]
    public IList<SvesTypeGroup> SvesType_Json
    {
      get => svesType;
      set => SvesType.Assign(value);
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of Do03.
    /// </summary>
    [JsonPropertyName("do03")]
    public FcrSvesAddress Do03
    {
      get => do03 ??= new();
      set => do03 = value;
    }

    /// <summary>
    /// A value of Cl02.
    /// </summary>
    [JsonPropertyName("cl02")]
    public FcrSvesAddress Cl02
    {
      get => cl02 ??= new();
      set => cl02 = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend FcrSvesTitleIiPend
    {
      get => fcrSvesTitleIiPend ??= new();
      set => fcrSvesTitleIiPend = value;
    }

    private WorkArea formattedT2ResponseDat;
    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea formattedReqDate;
    private WorkArea formattedRespRecDate;
    private WorkArea formattedFcrRecDate;
    private WorkArea formattedPrimarySsn;
    private WorkArea formattedVerifiedSsn;
    private WorkArea formattedOtherSsn;
    private WorkArea svesErrorMsgs;
    private Array<SvesTypeGroup> svesType;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesAddress do03;
    private FcrSvesAddress cl02;
    private FcrSvesTitleIiPend fcrSvesTitleIiPend;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public WorkArea Person
    {
      get => person ??= new();
      set => person = value;
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
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private WorkArea person;
    private CsePerson csePerson;
    private Common onTheCase;
    private Common supervisor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }
#endregion
}

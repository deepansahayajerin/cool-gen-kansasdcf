// Program: OE_FCR_SVES_PRISON_WEB, ID: 945075439, model: 746.
// Short name: SWCSV05P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_PRISON_WEB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class OeFcrSvesPrisonWeb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_PRISON_WEB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesPrisonWeb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesPrisonWeb.
  /// </summary>
  public OeFcrSvesPrisonWeb(IContext context, Import import, Export export):
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
    // 03/29/2018    JLH       CQ61705    Remove edit that only allows case 
    // worker and supervisor to see SVES information.
    // *********************************************************
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

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeFcrSvesPrison();
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
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
    target.TransmitterStateTerritoryCode = source.TransmitterStateTerritoryCode;
    target.SubmittedFirstName = source.SubmittedFirstName;
    target.SubmittedMiddleName = source.SubmittedMiddleName;
    target.SubmittedLastName = source.SubmittedLastName;
  }

  private static void MoveFcrSvesGenInfo2(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.ReturnedFirstName = source.ReturnedFirstName;
    target.ReturnedMiddleName = source.ReturnedMiddleName;
    target.ReturnedLastName = source.ReturnedLastName;
    target.SexCode = source.SexCode;
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
    export.FormattedRespDate.Text10 = useExport.FormattedRespRecDate.Text10;
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    useExport.SvesType.CopyTo(export.SvesType, MoveSvesType);
    MoveFcrSvesGenInfo1(useExport.FcrSvesGenInfo, export.FcrSvesGenInfo);
  }

  private void UseOeFcrSvesPrison()
  {
    var useImport = new OeFcrSvesPrison.Import();
    var useExport = new OeFcrSvesPrison.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesPrison.Execute, useImport, useExport);

    export.FormattedReportDate.Text10 = useExport.FormattedReportDate.Text10;
    export.FormattedReleaseDate.Text10 = useExport.FormattedReleaseDate.Text10;
    export.FormattedConfineDate.Text10 = useExport.FormattedConfineDate.Text10;
    export.FormattedDob.Text10 = useExport.FormattedDob.Text10;
    export.FormattedFax.Text12 = useExport.FormattedFax.Text12;
    export.FormattedPhone.Text12 = useExport.FormattedPhone.Text12;
    export.FormattedReportedSsn.Text11 = useExport.FormattedReportedSsn.Text11;
    export.FormattedMultiSsn.Text11 = useExport.FormattedMultiSsn.Text11;
    export.FormattedSsn.Text11 = useExport.FormattedSsn.Text11;
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    export.FcrSvesAddress.Assign(useExport.FcrSvesAddress);
    MoveFcrSvesGenInfo2(useExport.FcrSvesGenInfo, export.FcrSvesGenInfo);
    export.FcrSvesPrison.Assign(useExport.FcrSvesPrison);
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
    /// A value of FormattedRespDate.
    /// </summary>
    [JsonPropertyName("formattedRespDate")]
    public WorkArea FormattedRespDate
    {
      get => formattedRespDate ??= new();
      set => formattedRespDate = value;
    }

    /// <summary>
    /// A value of FormattedDob.
    /// </summary>
    [JsonPropertyName("formattedDob")]
    public WorkArea FormattedDob
    {
      get => formattedDob ??= new();
      set => formattedDob = value;
    }

    /// <summary>
    /// A value of FormattedConfineDate.
    /// </summary>
    [JsonPropertyName("formattedConfineDate")]
    public WorkArea FormattedConfineDate
    {
      get => formattedConfineDate ??= new();
      set => formattedConfineDate = value;
    }

    /// <summary>
    /// A value of FormattedReleaseDate.
    /// </summary>
    [JsonPropertyName("formattedReleaseDate")]
    public WorkArea FormattedReleaseDate
    {
      get => formattedReleaseDate ??= new();
      set => formattedReleaseDate = value;
    }

    /// <summary>
    /// A value of FormattedReportDate.
    /// </summary>
    [JsonPropertyName("formattedReportDate")]
    public WorkArea FormattedReportDate
    {
      get => formattedReportDate ??= new();
      set => formattedReportDate = value;
    }

    /// <summary>
    /// A value of FormattedSsn.
    /// </summary>
    [JsonPropertyName("formattedSsn")]
    public WorkArea FormattedSsn
    {
      get => formattedSsn ??= new();
      set => formattedSsn = value;
    }

    /// <summary>
    /// A value of FormattedMultiSsn.
    /// </summary>
    [JsonPropertyName("formattedMultiSsn")]
    public WorkArea FormattedMultiSsn
    {
      get => formattedMultiSsn ??= new();
      set => formattedMultiSsn = value;
    }

    /// <summary>
    /// A value of FormattedReportedSsn.
    /// </summary>
    [JsonPropertyName("formattedReportedSsn")]
    public WorkArea FormattedReportedSsn
    {
      get => formattedReportedSsn ??= new();
      set => formattedReportedSsn = value;
    }

    /// <summary>
    /// A value of FormattedPhone.
    /// </summary>
    [JsonPropertyName("formattedPhone")]
    public TextWorkArea FormattedPhone
    {
      get => formattedPhone ??= new();
      set => formattedPhone = value;
    }

    /// <summary>
    /// A value of FormattedFax.
    /// </summary>
    [JsonPropertyName("formattedFax")]
    public TextWorkArea FormattedFax
    {
      get => formattedFax ??= new();
      set => formattedFax = value;
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
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
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
    /// A value of FcrSvesPrison.
    /// </summary>
    [JsonPropertyName("fcrSvesPrison")]
    public FcrSvesPrison FcrSvesPrison
    {
      get => fcrSvesPrison ??= new();
      set => fcrSvesPrison = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea formattedReqDate;
    private WorkArea formattedRespDate;
    private WorkArea formattedDob;
    private WorkArea formattedConfineDate;
    private WorkArea formattedReleaseDate;
    private WorkArea formattedReportDate;
    private WorkArea formattedSsn;
    private WorkArea formattedMultiSsn;
    private WorkArea formattedReportedSsn;
    private TextWorkArea formattedPhone;
    private TextWorkArea formattedFax;
    private WorkArea svesErrorMsgs;
    private Array<SvesTypeGroup> svesType;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesPrison fcrSvesPrison;
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

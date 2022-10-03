// Program: OE_FCR_SVES_T2_WEB, ID: 945077296, model: 746.
// Short name: SWCSV03P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T2_WEB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class OeFcrSvesT2Web: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T2_WEB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT2Web(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT2Web.
  /// </summary>
  public OeFcrSvesT2Web(IContext context, Import import, Export export):
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
    // 03/28/2018   JLH     CQ61705    Remove edit that only allows case worker 
    // and supervisor to see SVES information.
    // *****************************************************************
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
      UseOeFcrSvesT2();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;
    }
  }

  private static void MoveFcrSvesAddress(FcrSvesAddress source,
    FcrSvesAddress target)
  {
    target.AddressLine1 = source.AddressLine1;
    target.AddressLine2 = source.AddressLine2;
    target.AddressLine3 = source.AddressLine3;
    target.AddressLine4 = source.AddressLine4;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
  }

  private static void MoveMbc(OeFcrSvesT2.Export.MbcGroup source,
    Export.MbcGroup target)
  {
    target.GmbcType.MbcType1 = source.GmbcType.MbcType1;
    target.GmbcAmount.MbcAmount1 = source.GmbcAmount.MbcAmount1;
    target.FormattedMbcDate.Text10 = source.FormattedMbcDate.Text10;
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
    export.FcrSvesGenInfo.Assign(useExport.FcrSvesGenInfo);
  }

  private void UseOeFcrSvesT2()
  {
    var useImport = new OeFcrSvesT2.Import();
    var useExport = new OeFcrSvesT2.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesT2.Execute, useImport, useExport);

    export.FormattedSmiStopDate.Text10 = useExport.FormattedSmiStopDate.Text10;
    export.FormattedSmiStartDate.Text10 =
      useExport.FormattedSmiStartDate.Text10;
    export.FormattedHiStopDate.Text10 = useExport.FormattedHiStopDate.Text10;
    export.ExportedFormattedHiStartDate.Text10 =
      useExport.ExportedFormattedHiStartDate.Text10;
    export.FormattedSuspDate.Text10 = useExport.FormattedSuspDate.Text10;
    export.FormattedCurEntDate.Text10 = useExport.FormattedCurEntDate.Text10;
    export.FormattedInitEntDate.Text10 = useExport.FormattedInitEntDate.Text10;
    export.FormattedDefpayDate.Text10 = useExport.FormattedDefpayDate.Text10;
    useExport.Mbc.CopyTo(export.Mbc, MoveMbc);
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    MoveFcrSvesAddress(useExport.FcrSvesAddress, export.FcrSvesAddress);
    export.FcrSvesTitleIi.Assign(useExport.FcrSvesTitleIi);
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

    /// <summary>A MbcGroup group.</summary>
    [Serializable]
    public class MbcGroup
    {
      /// <summary>
      /// A value of GmbcType.
      /// </summary>
      [JsonPropertyName("gmbcType")]
      public FcrSvesTitleIi GmbcType
      {
        get => gmbcType ??= new();
        set => gmbcType = value;
      }

      /// <summary>
      /// A value of GmbcAmount.
      /// </summary>
      [JsonPropertyName("gmbcAmount")]
      public FcrSvesTitleIi GmbcAmount
      {
        get => gmbcAmount ??= new();
        set => gmbcAmount = value;
      }

      /// <summary>
      /// A value of FormattedMbcDate.
      /// </summary>
      [JsonPropertyName("formattedMbcDate")]
      public WorkArea FormattedMbcDate
      {
        get => formattedMbcDate ??= new();
        set => formattedMbcDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private FcrSvesTitleIi gmbcType;
      private FcrSvesTitleIi gmbcAmount;
      private WorkArea formattedMbcDate;
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
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of FormattedSmiStopDate.
    /// </summary>
    [JsonPropertyName("formattedSmiStopDate")]
    public WorkArea FormattedSmiStopDate
    {
      get => formattedSmiStopDate ??= new();
      set => formattedSmiStopDate = value;
    }

    /// <summary>
    /// A value of FormattedSmiStartDate.
    /// </summary>
    [JsonPropertyName("formattedSmiStartDate")]
    public WorkArea FormattedSmiStartDate
    {
      get => formattedSmiStartDate ??= new();
      set => formattedSmiStartDate = value;
    }

    /// <summary>
    /// A value of FormattedHiStopDate.
    /// </summary>
    [JsonPropertyName("formattedHiStopDate")]
    public WorkArea FormattedHiStopDate
    {
      get => formattedHiStopDate ??= new();
      set => formattedHiStopDate = value;
    }

    /// <summary>
    /// A value of ExportedFormattedHiStartDate.
    /// </summary>
    [JsonPropertyName("exportedFormattedHiStartDate")]
    public WorkArea ExportedFormattedHiStartDate
    {
      get => exportedFormattedHiStartDate ??= new();
      set => exportedFormattedHiStartDate = value;
    }

    /// <summary>
    /// A value of FormattedSuspDate.
    /// </summary>
    [JsonPropertyName("formattedSuspDate")]
    public WorkArea FormattedSuspDate
    {
      get => formattedSuspDate ??= new();
      set => formattedSuspDate = value;
    }

    /// <summary>
    /// A value of FormattedCurEntDate.
    /// </summary>
    [JsonPropertyName("formattedCurEntDate")]
    public WorkArea FormattedCurEntDate
    {
      get => formattedCurEntDate ??= new();
      set => formattedCurEntDate = value;
    }

    /// <summary>
    /// A value of FormattedInitEntDate.
    /// </summary>
    [JsonPropertyName("formattedInitEntDate")]
    public WorkArea FormattedInitEntDate
    {
      get => formattedInitEntDate ??= new();
      set => formattedInitEntDate = value;
    }

    /// <summary>
    /// A value of FormattedDefpayDate.
    /// </summary>
    [JsonPropertyName("formattedDefpayDate")]
    public WorkArea FormattedDefpayDate
    {
      get => formattedDefpayDate ??= new();
      set => formattedDefpayDate = value;
    }

    /// <summary>
    /// Gets a value of Mbc.
    /// </summary>
    [JsonIgnore]
    public Array<MbcGroup> Mbc => mbc ??= new(MbcGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Mbc for json serialization.
    /// </summary>
    [JsonPropertyName("mbc")]
    [Computed]
    public IList<MbcGroup> Mbc_Json
    {
      get => mbc;
      set => Mbc.Assign(value);
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
    /// A value of FcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIi")]
    public FcrSvesTitleIi FcrSvesTitleIi
    {
      get => fcrSvesTitleIi ??= new();
      set => fcrSvesTitleIi = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea formattedReqDate;
    private WorkArea formattedRespDate;
    private WorkArea svesErrorMsgs;
    private Array<SvesTypeGroup> svesType;
    private WorkArea formattedDate;
    private WorkArea formattedSmiStopDate;
    private WorkArea formattedSmiStartDate;
    private WorkArea formattedHiStopDate;
    private WorkArea exportedFormattedHiStartDate;
    private WorkArea formattedSuspDate;
    private WorkArea formattedCurEntDate;
    private WorkArea formattedInitEntDate;
    private WorkArea formattedDefpayDate;
    private Array<MbcGroup> mbc;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesTitleIi fcrSvesTitleIi;
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

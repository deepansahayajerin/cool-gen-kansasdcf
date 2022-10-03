// Program: OE_FCR_SVES_PERSON_RECORD, ID: 945074685, model: 746.
// Short name: SWE03665
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_PERSON_RECORD.
/// </summary>
[Serializable]
public partial class OeFcrSvesPersonRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_PERSON_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesPersonRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesPersonRecord.
  /// </summary>
  public OeFcrSvesPersonRecord(IContext context, Import import, Export export):
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
    // ***********************************************************************
    // ****************************************************************
    // Check for input of agency code.
    // If has input retrieve the data for that agency type,
    // otherwise retrieve all agency types available for person
    // and the corresponding data for the first one of the group.
    // ****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************************************************************************
    // Add 5 places to the beginning of the  person number to match up to the 
    // member id which is 15 places
    // (the member id coming in will only use the last 10 places for the member 
    // id with the first 5 as zeros).
    // ***********************************************************************************************************
    local.Person.Text15 = "00000" + import.CsePersonsWorkSet.Number;

    if (IsEmpty(import.FcrSvesGenInfo.LocateSourceResponseAgencyCo))
    {
      export.SvesType.Index = -1;

      foreach(var item in ReadFcrSvesGenInfo2())
      {
        if (export.SvesType.Index + 1 >= Export.SvesTypeGroup.Capacity)
        {
          return;
        }

        ++export.SvesType.Index;
        export.SvesType.CheckSize();

        if (export.SvesType.Index == 0)
        {
          export.FcrSvesGenInfo.Assign(entities.Existing);
          local.DateWorkArea.Date = entities.Existing.RequestDate;
          UseCabFormatDateOnline1();
          local.DateWorkArea.Date = entities.Existing.ResponseReceivedDate;
          UseCabFormatDateOnline2();
        }

        export.SvesType.Update.Gtype.LocateSourceResponseAgencyCo =
          entities.Existing.LocateSourceResponseAgencyCo;
      }
    }
    else
    {
      if (ReadFcrSvesGenInfo1())
      {
        export.FcrSvesGenInfo.Assign(entities.Existing);
        local.DateWorkArea.Date = entities.Existing.RequestDate;
        UseCabFormatDateOnline1();
        local.DateWorkArea.Date = entities.Existing.ResponseReceivedDate;
        UseCabFormatDateOnline2();
      }
      else
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        return;
      }

      export.SvesType.Index = -1;

      foreach(var item in ReadFcrSvesGenInfo2())
      {
        if (export.SvesType.Index + 1 >= Export.SvesTypeGroup.Capacity)
        {
          return;
        }

        ++export.SvesType.Index;
        export.SvesType.CheckSize();

        export.SvesType.Update.Gtype.LocateSourceResponseAgencyCo =
          entities.Existing.LocateSourceResponseAgencyCo;
      }
    }
  }

  private void UseCabFormatDateOnline1()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedReqDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDateOnline2()
  {
    var useImport = new CabFormatDateOnline.Import();
    var useExport = new CabFormatDateOnline.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDateOnline.Execute, useImport, useExport);

    export.FormattedRespRecDate.Text10 = useExport.FormattedDate.Text10;
  }

  private bool ReadFcrSvesGenInfo1()
  {
    entities.Existing.Populated = false;

    return Read("ReadFcrSvesGenInfo1",
      (db, command) =>
      {
        db.SetString(command, "memberId", local.Person.Text15);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.Existing.MemberId = db.GetString(reader, 0);
        entities.Existing.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.Existing.TransmitterStateTerritoryCode =
          db.GetNullableString(reader, 2);
        entities.Existing.RequestDate = db.GetNullableDate(reader, 3);
        entities.Existing.ResponseReceivedDate = db.GetNullableDate(reader, 4);
        entities.Existing.SubmittedFirstName = db.GetNullableString(reader, 5);
        entities.Existing.SubmittedMiddleName = db.GetNullableString(reader, 6);
        entities.Existing.SubmittedLastName = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFcrSvesGenInfo2()
  {
    entities.Existing.Populated = false;

    return ReadEach("ReadFcrSvesGenInfo2",
      (db, command) =>
      {
        db.SetString(command, "memberId", local.Person.Text15);
      },
      (db, reader) =>
      {
        entities.Existing.MemberId = db.GetString(reader, 0);
        entities.Existing.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.Existing.TransmitterStateTerritoryCode =
          db.GetNullableString(reader, 2);
        entities.Existing.RequestDate = db.GetNullableDate(reader, 3);
        entities.Existing.ResponseReceivedDate = db.GetNullableDate(reader, 4);
        entities.Existing.SubmittedFirstName = db.GetNullableString(reader, 5);
        entities.Existing.SubmittedMiddleName = db.GetNullableString(reader, 6);
        entities.Existing.SubmittedLastName = db.GetNullableString(reader, 7);
        entities.Existing.Populated = true;

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
      /// A value of Gtype.
      /// </summary>
      [JsonPropertyName("gtype")]
      public FcrSvesGenInfo Gtype
      {
        get => gtype ??= new();
        set => gtype = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private FcrSvesGenInfo gtype;
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

    private WorkArea formattedReqDate;
    private WorkArea formattedRespRecDate;
    private WorkArea svesErrorMsgs;
    private Array<SvesTypeGroup> svesType;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PopulatePlaceholder.
    /// </summary>
    [JsonPropertyName("populatePlaceholder")]
    public Common PopulatePlaceholder
    {
      get => populatePlaceholder ??= new();
      set => populatePlaceholder = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Date15.
    /// </summary>
    [JsonPropertyName("date15")]
    public WorkArea Date15
    {
      get => date15 ??= new();
      set => date15 = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private Common populatePlaceholder;
    private WorkArea formattedDate;
    private DateWorkArea dateWorkArea;
    private WorkArea date15;
    private WorkArea person;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public FcrSvesGenInfo Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private FcrSvesGenInfo existing;
  }
#endregion
}

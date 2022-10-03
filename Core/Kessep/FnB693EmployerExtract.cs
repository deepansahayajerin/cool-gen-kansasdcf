// Program: FN_B693_EMPLOYER_EXTRACT, ID: 374401305, model: 746.
// Short name: SWEF693B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B693_EMPLOYER_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB693EmployerExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B693_EMPLOYER_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB693EmployerExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB693EmployerExtract.
  /// </summary>
  public FnB693EmployerExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************
    // Initial Version - 03/16/00 SWETTREM
    // ************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB693Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadEmployerEmployerAddress())
    {
      ++local.EmployerRecsRead.Count;

      // **********************************
      // Print the employer record
      // **********************************
      local.EmployerRecord.Name = entities.Employer.Name ?? Spaces(50);
      local.EmployerRecord.Street1 = entities.EmployerAddress.Street1 ?? Spaces
        (50);
      local.EmployerRecord.Street2 = entities.EmployerAddress.Street2 ?? Spaces
        (50);
      local.EmployerRecord.Street3 = entities.EmployerAddress.Street3 ?? Spaces
        (50);
      local.EmployerRecord.City = entities.EmployerAddress.City ?? Spaces(20);
      local.EmployerRecord.State = entities.EmployerAddress.State ?? Spaces(2);
      local.EmployerRecord.ZipCode = entities.EmployerAddress.ZipCode ?? Spaces
        (5);
      local.EmployerRecord.PhoneNumber =
        NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13, 3) +
        entities.Employer.PhoneNo;
      local.EmployerRecord.Ein = entities.Employer.Ein ?? Spaces(10);
      local.KpcExternalParms.Parm1 = "GR";
      UseFnExtWriteEmployerFile2();

      if (IsEmpty(local.KpcExternalParms.Parm1))
      {
        ++local.EmployerRecsProcessed.Count;
      }
      else
      {
        ExitState = "FN0000_ERROR_WRITING_EMP_FILE";
        UseFnB693PrintErrorLine1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }
    }

    // ****************************
    // CLOSE OUTPUT FILE
    // ****************************
    local.KpcExternalParms.Parm1 = "CF";
    UseFnExtWriteEmployerFile1();
    local.CloseInd.Flag = "Y";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // : Close the Error Report.
      UseFnB693PrintErrorLine2();
      UseFnB693PrintControl();
    }
    else
    {
      // : Report the error that occurred and close the Error Report.
      //   ABEND the procedure once reporting is complete.
      UseFnB693PrintErrorLine2();
      UseFnB693PrintControl();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveKpcExternalParms(KpcExternalParms source,
    KpcExternalParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseFnB693Housekeeping()
  {
    var useImport = new FnB693Housekeeping.Import();
    var useExport = new FnB693Housekeeping.Export();

    Call(FnB693Housekeeping.Execute, useImport, useExport);

    local.CurrentRun.Date = useExport.CurrentRun.Date;
  }

  private void UseFnB693PrintControl()
  {
    var useImport = new FnB693PrintControl.Import();
    var useExport = new FnB693PrintControl.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;
    useImport.EmployerRecsRead.Count = local.EmployerRecsRead.Count;
    useImport.EmploerRecsProcessed.Count = local.EmployerRecsProcessed.Count;

    Call(FnB693PrintControl.Execute, useImport, useExport);
  }

  private void UseFnB693PrintErrorLine1()
  {
    var useImport = new FnB693PrintErrorLine.Import();
    var useExport = new FnB693PrintErrorLine.Export();

    useImport.Employer.Assign(entities.Employer);
    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB693PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnB693PrintErrorLine2()
  {
    var useImport = new FnB693PrintErrorLine.Import();
    var useExport = new FnB693PrintErrorLine.Export();

    useImport.CloseInd.Flag = local.CloseInd.Flag;

    Call(FnB693PrintErrorLine.Execute, useImport, useExport);
  }

  private void UseFnExtWriteEmployerFile1()
  {
    var useImport = new FnExtWriteEmployerFile.Import();
    var useExport = new FnExtWriteEmployerFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteEmployerFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private void UseFnExtWriteEmployerFile2()
  {
    var useImport = new FnExtWriteEmployerFile.Import();
    var useExport = new FnExtWriteEmployerFile.Export();

    MoveKpcExternalParms(local.KpcExternalParms, useImport.KpcExternalParms);
    useImport.EmployerRecord.Assign(local.EmployerRecord);
    MoveKpcExternalParms(local.KpcExternalParms, useExport.KpcExternalParms);

    Call(FnExtWriteEmployerFile.Execute, useImport, useExport);

    MoveKpcExternalParms(useExport.KpcExternalParms, local.KpcExternalParms);
  }

  private IEnumerable<bool> ReadEmployerEmployerAddress()
  {
    entities.Employer.Populated = false;
    entities.EmployerAddress.Populated = false;

    return ReadEach("ReadEmployerEmployerAddress",
      null,
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.EmployerAddress.LocationType = db.GetString(reader, 5);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 7);
        entities.EmployerAddress.City = db.GetNullableString(reader, 8);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 9);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 10);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 11);
        entities.EmployerAddress.State = db.GetNullableString(reader, 12);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 13);
        entities.Employer.Populated = true;
        entities.EmployerAddress.Populated = true;

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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentRun.
    /// </summary>
    [JsonPropertyName("currentRun")]
    public DateWorkArea CurrentRun
    {
      get => currentRun ??= new();
      set => currentRun = value;
    }

    /// <summary>
    /// A value of CloseInd.
    /// </summary>
    [JsonPropertyName("closeInd")]
    public Common CloseInd
    {
      get => closeInd ??= new();
      set => closeInd = value;
    }

    /// <summary>
    /// A value of EmployerRecsRead.
    /// </summary>
    [JsonPropertyName("employerRecsRead")]
    public Common EmployerRecsRead
    {
      get => employerRecsRead ??= new();
      set => employerRecsRead = value;
    }

    /// <summary>
    /// A value of EmployerRecsProcessed.
    /// </summary>
    [JsonPropertyName("employerRecsProcessed")]
    public Common EmployerRecsProcessed
    {
      get => employerRecsProcessed ??= new();
      set => employerRecsProcessed = value;
    }

    /// <summary>
    /// A value of KpcExternalParms.
    /// </summary>
    [JsonPropertyName("kpcExternalParms")]
    public KpcExternalParms KpcExternalParms
    {
      get => kpcExternalParms ??= new();
      set => kpcExternalParms = value;
    }

    /// <summary>
    /// A value of EmployerRecord.
    /// </summary>
    [JsonPropertyName("employerRecord")]
    public EmployerRecord EmployerRecord
    {
      get => employerRecord ??= new();
      set => employerRecord = value;
    }

    private DateWorkArea currentRun;
    private Common closeInd;
    private Common employerRecsRead;
    private Common employerRecsProcessed;
    private KpcExternalParms kpcExternalParms;
    private EmployerRecord employerRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private Employer employer;
    private EmployerAddress employerAddress;
  }
#endregion
}

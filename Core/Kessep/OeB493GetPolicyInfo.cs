// Program: OE_B493_GET_POLICY_INFO, ID: 371177206, model: 746.
// Short name: SWE02484
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B493_GET_POLICY_INFO.
/// </summary>
[Serializable]
public partial class OeB493GetPolicyInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B493_GET_POLICY_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB493GetPolicyInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB493GetPolicyInfo.
  /// </summary>
  public OeB493GetPolicyInfo(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.EabFileHandling.Action = "WRITE";
    export.OneOrMoreCodesFound.Flag = "N";

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode1))
    {
      if (ReadCodeValue1())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode1 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode1 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode2))
    {
      if (ReadCodeValue2())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode2 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode2 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode3))
    {
      if (ReadCodeValue3())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode3 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode3 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode4))
    {
      if (ReadCodeValue4())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode4 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode4 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode5))
    {
      if (ReadCodeValue5())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode5 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode5 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode6))
    {
      if (ReadCodeValue6())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode6 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode6 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }

    if (!IsEmpty(import.HealthInsuranceCoverage.CoverageCode7))
    {
      if (ReadCodeValue7())
      {
        export.OneOrMoreCodesFound.Flag = "Y";
        export.HealthInsuranceCoverage.CoverageCode7 =
          Substring(entities.CodeValue.Description, 1, 1);
      }
      else
      {
        local.NeededToWrite.RptDetail = "Coverage code: " + (
          import.HealthInsuranceCoverage.CoverageCode7 ?? "") + " not in code value table called: EDS COVERAGES";
          
        UseCabErrorReport();
      }
    }
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCodeValue1()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode1",
          import.HealthInsuranceCoverage.CoverageCode1 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue2()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode2",
          import.HealthInsuranceCoverage.CoverageCode2 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue3()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode3",
          import.HealthInsuranceCoverage.CoverageCode3 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue4()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode4",
          import.HealthInsuranceCoverage.CoverageCode4 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue5()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode5",
          import.HealthInsuranceCoverage.CoverageCode5 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue6()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue6",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode6",
          import.HealthInsuranceCoverage.CoverageCode6 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCodeValue7()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue7",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", import.Coverages.Id);
        db.SetNullableString(
          command, "coverageCode7",
          import.HealthInsuranceCoverage.CoverageCode7 ?? "");
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.CodeValue.Description = db.GetString(reader, 5);
        entities.CodeValue.Populated = true;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of Coverages.
    /// </summary>
    [JsonPropertyName("coverages")]
    public Code Coverages
    {
      get => coverages ??= new();
      set => coverages = value;
    }

    private DateWorkArea processDate;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Code coverages;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OneOrMoreCodesFound.
    /// </summary>
    [JsonPropertyName("oneOrMoreCodesFound")]
    public Common OneOrMoreCodesFound
    {
      get => oneOrMoreCodesFound ??= new();
      set => oneOrMoreCodesFound = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    private Common oneOrMoreCodesFound;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    private CodeValue codeValue;
    private Code code;
  }
#endregion
}

// Program: SI_B273_CREATE_EMP_INCOME_SOURCE, ID: 371059263, model: 746.
// Short name: SWE01271
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_CREATE_EMP_INCOME_SOURCE.
/// </summary>
[Serializable]
public partial class SiB273CreateEmpIncomeSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_CREATE_EMP_INCOME_SOURCE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273CreateEmpIncomeSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273CreateEmpIncomeSource.
  /// </summary>
  public SiB273CreateEmpIncomeSource(IContext context, Import import,
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
    // ******************************************************************************
    // Military and Employment are subtypes of Income Source
    // ******************************************************************************
    // ******************************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ******************************************************************************
    // 
    // *    DATE       NAME      REQUEST      DESCRIPTION
    // *
    // 
    // * ----------  ---------  ---------
    // ----------------------------------------*
    // * 08/07/2003  Bonnie Lee
    // PR185104   New value of 'P' for military status and*
    // *
    // new value of 'Pension/Retired' for
    // note.*
    // 
    // *
    // 
    // *
    // ******************************************************************************
    local.DateWorkArea.Timestamp = Now();
    export.IncomeSourceCreated.Count = import.IncomeSourceCreated.Count;

    if (ReadCsePerson())
    {
      if (!ReadEmployer())
      {
        ExitState = "EMPLOYER_NF";

        return;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ***************************************************************************************
    // 08/07/2003   B. Lee
    // 
    // Added check for new vaule of 'P' for military status and set note to '
    // Pension/Retired'.
    // ***************************************************************************************
    if (AsChar(import.MilitaryStatus.Text1) == 'A' || AsChar
      (import.MilitaryStatus.Text1) == 'R' || AsChar
      (import.MilitaryStatus.Text1) == 'P')
    {
      if (IsEmpty(import.Employment.Note))
      {
        if (AsChar(import.MilitaryStatus.Text1) == 'A')
        {
          local.Military.Note = "Active Service";
        }
        else if (AsChar(import.MilitaryStatus.Text1) == 'R')
        {
          local.Military.Note = "Reserves";
        }
        else
        {
          local.Military.Note = "Pension/Retired";
        }
      }
      else
      {
        local.Military.Note = import.Employment.Note ?? "";
      }

      try
      {
        CreateMilitary();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++export.IncomeSourceCreated.Count;

      // ***************************************************************
      // 11/20/1998   C. Ott
      // This batch program creates data that may be maintained online by the 
      // INCS screen.  Because that screen requires the presence of an 'HP' and
      // an 'HF' Income Source Contact type, they must be created here to ensure
      // functioning of the INCS screen.  This is an issue that may be re-
      // consider in the future but must take into account the requirements of
      // both this batch procedure and the INCS screen.
      // ****************************************************************
      local.IncomeSourceContact.AreaCode = entities.Employer.AreaCode;
      local.IncomeSourceContact.Number =
        (int?)StringToNumber(entities.Employer.PhoneNo);
      local.IncomeSourceContact.Type1 = "HP";
      local.CsePerson.Number = import.CsePerson.Number;
      UseSiIncsCreateIncomeSrcContct2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "DATABASE ERROR CREATING INCOME SOURCE CONTACT FOR CSE PERSON = " + local
          .CsePerson.Number + ", EMPLOYER KANSAS ID = " + entities
          .Employer.KansasId;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.IncomeSourceContact.AreaCode = 0;
      local.IncomeSourceContact.Number = 0;
      local.IncomeSourceContact.Type1 = "HF";
      UseSiIncsCreateIncomeSrcContct2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "DATABASE ERROR CREATING INCOME SOURCE CONTACT FOR CSE PERSON = " + local
          .CsePerson.Number + ", EMPLOYER KANSAS ID = " + entities
          .Employer.KansasId;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
    else
    {
      try
      {
        CreateEmployment();

        if (AsChar(import.AddressSuitableForIwo.Flag) == 'Y' && AsChar
          (import.AutomaticGenerateIwo.Flag) == 'Y')
        {
          UseLeAutomaticIwoGeneration();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail =
              "UNABLE TO CREATE IWO FOR PERSON:  " + local.CsePerson.Number + " ERR: " +
              local.ExitStateWorkArea.Message;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INCOME_SOURCE_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INCOME_SOURCE_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++export.IncomeSourceCreated.Count;

      // ***************************************************************
      // 11/20/1998   C. Ott
      // This batch program creates data that may be maintained online by the 
      // INCS screen.  Because that screen requires the presence of an 'HP' and
      // an 'HF' Income Source Contact type, they must be created here to ensure
      // functioning of the INCS screen.  This is an issue that may be re-
      // consider in the future but must take into account the requirements of
      // both this batch procedure and the INCS screen.
      // ****************************************************************
      local.IncomeSourceContact.AreaCode = entities.Employer.AreaCode;
      local.IncomeSourceContact.Number =
        (int?)StringToNumber(entities.Employer.PhoneNo);
      local.IncomeSourceContact.Type1 = "HP";
      local.CsePerson.Number = import.CsePerson.Number;
      UseSiIncsCreateIncomeSrcContct1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "DATABASE ERROR CREATING INCOME SOURCE CONTACT FOR CSE PERSON = " + local
          .CsePerson.Number + ", EMPLOYER KANSAS ID = " + entities
          .Employer.KansasId;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.IncomeSourceContact.AreaCode = 0;
      local.IncomeSourceContact.Number = 0;
      local.IncomeSourceContact.Type1 = "HF";
      UseSiIncsCreateIncomeSrcContct1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "DATABASE ERROR CREATING INCOME SOURCE CONTACT FOR CSE PERSON = " + local
          .CsePerson.Number + ", EMPLOYER KANSAS ID = " + entities
          .Employer.KansasId;
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.ReturnCd = source.ReturnCd;
    target.StartDt = source.StartDt;
  }

  private static void MoveIncomeSourceContact(IncomeSourceContact source,
    IncomeSourceContact target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.ExtensionNo = source.ExtensionNo;
    target.Number = source.Number;
    target.Type1 = source.Type1;
    target.AreaCode = source.AreaCode;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseLeAutomaticIwoGeneration()
  {
    var useImport = new LeAutomaticIwoGeneration.Import();
    var useExport = new LeAutomaticIwoGeneration.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveIncomeSource(entities.Employment, useImport.IncomeSource);

    Call(LeAutomaticIwoGeneration.Execute, useImport, useExport);
  }

  private void UseSiIncsCreateIncomeSrcContct1()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.IncomeSource.Identifier = entities.Employment.Identifier;
    MoveIncomeSourceContact(local.IncomeSourceContact,
      useImport.IncomeSourceContact);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    local.IncomeSourceContact.Assign(useImport.IncomeSourceContact);
    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void UseSiIncsCreateIncomeSrcContct2()
  {
    var useImport = new SiIncsCreateIncomeSrcContct.Import();
    var useExport = new SiIncsCreateIncomeSrcContct.Export();

    useImport.IncomeSource.Identifier = entities.Military.Identifier;
    MoveIncomeSourceContact(local.IncomeSourceContact,
      useImport.IncomeSourceContact);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiIncsCreateIncomeSrcContct.Execute, useImport, useExport);

    local.IncomeSourceContact.Assign(useImport.IncomeSourceContact);
    local.CsePerson.Number = useImport.CsePerson.Number;
  }

  private void CreateEmployment()
  {
    var identifier = local.DateWorkArea.Timestamp;
    var type1 = "E";
    var lastQtrIncome = import.Employment.LastQtrIncome.GetValueOrDefault();
    var lastQtr = import.Employment.LastQtr ?? "";
    var lastQtrYr = import.Employment.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      import.Employment.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = import.Employment.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      import.Employment.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      import.Employment.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = import.Employment.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      import.Employment.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      import.Employment.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = import.Employment.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      import.Employment.Attribute4ThQtrYr.GetValueOrDefault();
    var sentDt = import.Employment.SentDt;
    var returnDt = import.Employment.ReturnDt;
    var returnCd = import.Employment.ReturnCd ?? "";
    var name = import.Employment.Name ?? "";
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var cspINumber = entities.CsePerson.Number;
    var selfEmployedInd = import.Employment.SelfEmployedInd ?? "";
    var empId = entities.Employer.Identifier;
    var sendTo = import.Employment.SendTo ?? "";
    var startDt = import.Employment.StartDt;
    var endDt = import.Employment.EndDt;
    var note = import.Employment.Note ?? "";
    var note2 = import.Employment.Note2 ?? "";

    CheckValid<IncomeSource>("Type1", type1);
    CheckValid<IncomeSource>("SendTo", sendTo);
    entities.Employment.Populated = false;
    Update("CreateEmployment",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableDecimal(command, "lastQtrIncome", lastQtrIncome);
        db.SetNullableString(command, "lastQtr", lastQtr);
        db.SetNullableInt32(command, "lastQtrYr", lastQtrYr);
        db.
          SetNullableDecimal(command, "secondQtrIncome", attribute2NdQtrIncome);
          
        db.SetNullableString(command, "secondQtr", attribute2NdQtr);
        db.SetNullableInt32(command, "secondQtrYr", attribute2NdQtrYr);
        db.SetNullableDecimal(command, "thirdQtrIncome", attribute3RdQtrIncome);
        db.SetNullableString(command, "thirdQtr", attribute3RdQtr);
        db.SetNullableInt32(command, "thirdQtrYr", attribute3RdQtrYr);
        db.
          SetNullableDecimal(command, "fourthQtrIncome", attribute4ThQtrIncome);
          
        db.SetNullableString(command, "fourthQtr", attribute4ThQtr);
        db.SetNullableInt32(command, "fourthQtrYr", attribute4ThQtrYr);
        db.SetNullableDate(command, "sentDt", sentDt);
        db.SetNullableDate(command, "returnDt", returnDt);
        db.SetNullableString(command, "returnCd", returnCd);
        db.SetNullableString(command, "name", name);
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", identifier);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "code", "");
        db.SetString(command, "cspINumber", cspINumber);
        db.SetNullableString(command, "selfEmployedInd", selfEmployedInd);
        db.SetNullableInt32(command, "empId", empId);
        db.SetString(command, "militaryCode", "");
        db.SetNullableString(command, "sendTo", sendTo);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "note2", note2);
      });

    entities.Employment.Identifier = identifier;
    entities.Employment.Type1 = type1;
    entities.Employment.LastQtrIncome = lastQtrIncome;
    entities.Employment.LastQtr = lastQtr;
    entities.Employment.LastQtrYr = lastQtrYr;
    entities.Employment.Attribute2NdQtrIncome = attribute2NdQtrIncome;
    entities.Employment.Attribute2NdQtr = attribute2NdQtr;
    entities.Employment.Attribute2NdQtrYr = attribute2NdQtrYr;
    entities.Employment.Attribute3RdQtrIncome = attribute3RdQtrIncome;
    entities.Employment.Attribute3RdQtr = attribute3RdQtr;
    entities.Employment.Attribute3RdQtrYr = attribute3RdQtrYr;
    entities.Employment.Attribute4ThQtrIncome = attribute4ThQtrIncome;
    entities.Employment.Attribute4ThQtr = attribute4ThQtr;
    entities.Employment.Attribute4ThQtrYr = attribute4ThQtrYr;
    entities.Employment.SentDt = sentDt;
    entities.Employment.ReturnDt = returnDt;
    entities.Employment.ReturnCd = returnCd;
    entities.Employment.Name = name;
    entities.Employment.LastUpdatedTimestamp = identifier;
    entities.Employment.LastUpdatedBy = lastUpdatedBy;
    entities.Employment.CreatedTimestamp = identifier;
    entities.Employment.CreatedBy = lastUpdatedBy;
    entities.Employment.CspINumber = cspINumber;
    entities.Employment.SelfEmployedInd = selfEmployedInd;
    entities.Employment.EmpId = empId;
    entities.Employment.SendTo = sendTo;
    entities.Employment.WorkerId = lastUpdatedBy;
    entities.Employment.StartDt = startDt;
    entities.Employment.EndDt = endDt;
    entities.Employment.Note = note;
    entities.Employment.Note2 = note2;
    entities.Employment.Populated = true;
  }

  private void CreateMilitary()
  {
    var identifier = local.DateWorkArea.Timestamp;
    var type1 = "M";
    var lastQtrIncome = import.Employment.LastQtrIncome.GetValueOrDefault();
    var lastQtr = import.Employment.LastQtr ?? "";
    var lastQtrYr = import.Employment.LastQtrYr.GetValueOrDefault();
    var attribute2NdQtrIncome =
      import.Employment.Attribute2NdQtrIncome.GetValueOrDefault();
    var attribute2NdQtr = import.Employment.Attribute2NdQtr ?? "";
    var attribute2NdQtrYr =
      import.Employment.Attribute2NdQtrYr.GetValueOrDefault();
    var attribute3RdQtrIncome =
      import.Employment.Attribute3RdQtrIncome.GetValueOrDefault();
    var attribute3RdQtr = import.Employment.Attribute3RdQtr ?? "";
    var attribute3RdQtrYr =
      import.Employment.Attribute3RdQtrYr.GetValueOrDefault();
    var attribute4ThQtrIncome =
      import.Employment.Attribute4ThQtrIncome.GetValueOrDefault();
    var attribute4ThQtr = import.Employment.Attribute4ThQtr ?? "";
    var attribute4ThQtrYr =
      import.Employment.Attribute4ThQtrYr.GetValueOrDefault();
    var sentDt = import.Employment.SentDt;
    var returnDt = import.Employment.ReturnDt;
    var returnCd = import.Employment.ReturnCd ?? "";
    var name = import.Employment.Name ?? "";
    var lastUpdatedBy = import.ProgramCheckpointRestart.ProgramName;
    var cspINumber = entities.CsePerson.Number;
    var empId = entities.Employer.Identifier;
    var militaryCode = import.MilitaryStatus.Text1;
    var sendTo = import.Employment.SendTo ?? "";
    var startDt = import.Employment.StartDt;
    var endDt = import.Employment.EndDt;
    var note = local.Military.Note ?? "";

    CheckValid<IncomeSource>("Type1", type1);
    CheckValid<IncomeSource>("SendTo", sendTo);
    entities.Military.Populated = false;
    Update("CreateMilitary",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "type", type1);
        db.SetNullableDecimal(command, "lastQtrIncome", lastQtrIncome);
        db.SetNullableString(command, "lastQtr", lastQtr);
        db.SetNullableInt32(command, "lastQtrYr", lastQtrYr);
        db.
          SetNullableDecimal(command, "secondQtrIncome", attribute2NdQtrIncome);
          
        db.SetNullableString(command, "secondQtr", attribute2NdQtr);
        db.SetNullableInt32(command, "secondQtrYr", attribute2NdQtrYr);
        db.SetNullableDecimal(command, "thirdQtrIncome", attribute3RdQtrIncome);
        db.SetNullableString(command, "thirdQtr", attribute3RdQtr);
        db.SetNullableInt32(command, "thirdQtrYr", attribute3RdQtrYr);
        db.
          SetNullableDecimal(command, "fourthQtrIncome", attribute4ThQtrIncome);
          
        db.SetNullableString(command, "fourthQtr", attribute4ThQtr);
        db.SetNullableInt32(command, "fourthQtrYr", attribute4ThQtrYr);
        db.SetNullableDate(command, "sentDt", sentDt);
        db.SetNullableDate(command, "returnDt", returnDt);
        db.SetNullableString(command, "returnCd", returnCd);
        db.SetNullableString(command, "name", name);
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", identifier);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetNullableString(command, "code", "");
        db.SetString(command, "cspINumber", cspINumber);
        db.SetNullableString(command, "selfEmployedInd", "");
        db.SetNullableInt32(command, "empId", empId);
        db.SetString(command, "militaryCode", militaryCode);
        db.SetNullableString(command, "sendTo", sendTo);
        db.SetNullableString(command, "workerId", lastUpdatedBy);
        db.SetNullableDate(command, "startDt", startDt);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetNullableString(command, "note", note);
        db.SetNullableString(command, "note2", "");
      });

    entities.Military.Identifier = identifier;
    entities.Military.Type1 = type1;
    entities.Military.LastQtrIncome = lastQtrIncome;
    entities.Military.LastQtr = lastQtr;
    entities.Military.LastQtrYr = lastQtrYr;
    entities.Military.Attribute2NdQtrIncome = attribute2NdQtrIncome;
    entities.Military.Attribute2NdQtr = attribute2NdQtr;
    entities.Military.Attribute2NdQtrYr = attribute2NdQtrYr;
    entities.Military.Attribute3RdQtrIncome = attribute3RdQtrIncome;
    entities.Military.Attribute3RdQtr = attribute3RdQtr;
    entities.Military.Attribute3RdQtrYr = attribute3RdQtrYr;
    entities.Military.Attribute4ThQtrIncome = attribute4ThQtrIncome;
    entities.Military.Attribute4ThQtr = attribute4ThQtr;
    entities.Military.Attribute4ThQtrYr = attribute4ThQtrYr;
    entities.Military.SentDt = sentDt;
    entities.Military.ReturnDt = returnDt;
    entities.Military.ReturnCd = returnCd;
    entities.Military.Name = name;
    entities.Military.LastUpdatedTimestamp = identifier;
    entities.Military.LastUpdatedBy = lastUpdatedBy;
    entities.Military.CreatedTimestamp = identifier;
    entities.Military.CreatedBy = lastUpdatedBy;
    entities.Military.CspINumber = cspINumber;
    entities.Military.EmpId = empId;
    entities.Military.MilitaryCode = militaryCode;
    entities.Military.SendTo = sendTo;
    entities.Military.WorkerId = lastUpdatedBy;
    entities.Military.StartDt = startDt;
    entities.Military.EndDt = endDt;
    entities.Military.Note = note;
    entities.Military.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.KansasId = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.CreatedBy = db.GetString(reader, 3);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 6);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 7);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 8);
        entities.Employer.Populated = true;
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
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of MilitaryStatus.
    /// </summary>
    [JsonPropertyName("militaryStatus")]
    public TextWorkArea MilitaryStatus
    {
      get => militaryStatus ??= new();
      set => militaryStatus = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of IncomeSourceCreated.
    /// </summary>
    [JsonPropertyName("incomeSourceCreated")]
    public Common IncomeSourceCreated
    {
      get => incomeSourceCreated ??= new();
      set => incomeSourceCreated = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    private Common automaticGenerateIwo;
    private Common addressSuitableForIwo;
    private TextWorkArea militaryStatus;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common incomeSourceCreated;
    private CsePerson csePerson;
    private Employer employer;
    private IncomeSource employment;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IncomeSourceCreated.
    /// </summary>
    [JsonPropertyName("incomeSourceCreated")]
    public Common IncomeSourceCreated
    {
      get => incomeSourceCreated ??= new();
      set => incomeSourceCreated = value;
    }

    private Common incomeSourceCreated;
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
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
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
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private IncomeSource military;
    private DateWorkArea dateWorkArea;
    private IncomeSourceContact incomeSourceContact;
    private CsePerson csePerson;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Employment.
    /// </summary>
    [JsonPropertyName("employment")]
    public IncomeSource Employment
    {
      get => employment ??= new();
      set => employment = value;
    }

    /// <summary>
    /// A value of Military.
    /// </summary>
    [JsonPropertyName("military")]
    public IncomeSource Military
    {
      get => military ??= new();
      set => military = value;
    }

    private Employer employer;
    private CsePerson csePerson;
    private IncomeSource employment;
    private IncomeSource military;
  }
#endregion
}

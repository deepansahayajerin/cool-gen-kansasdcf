// Program: FN_CREATE_OCSE157_VERIFICATION, ID: 371094636, model: 746.
// Short name: SWE02913
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_OCSE157_VERIFICATION.
/// </summary>
[Serializable]
public partial class FnCreateOcse157Verification: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_OCSE157_VERIFICATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateOcse157Verification(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateOcse157Verification.
  /// </summary>
  public FnCreateOcse157Verification(IContext context, Import import,
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
    for(local.NbrOfTries.Count = 1; local.NbrOfTries.Count <= 5; ++
      local.NbrOfTries.Count)
    {
      try
      {
        CreateOcse157Verification();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -------------------------------------------------------------
    // Primary Key is Created_ts. So we should never hit ae
    // condition. Hence no Exit State is necessary.
    // ------------------------------------------------------------
  }

  private void CreateOcse157Verification()
  {
    var fiscalYear = import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    var runNumber = import.Ocse157Verification.RunNumber.GetValueOrDefault();
    var lineNumber = import.Ocse157Verification.LineNumber ?? "";
    var column = import.Ocse157Verification.Column ?? "";
    var createdTimestamp = Now();
    var caseNumber = import.Ocse157Verification.CaseNumber ?? "";
    var suppPersonNumber = import.Ocse157Verification.SuppPersonNumber ?? "";
    var obligorPersonNbr = import.Ocse157Verification.ObligorPersonNbr ?? "";
    var dtePaternityEst = import.Ocse157Verification.DtePaternityEst;
    var courtOrderNumber = import.Ocse157Verification.CourtOrderNumber ?? "";
    var legalCreatedDte = import.Ocse157Verification.LegalCreatedDte;
    var dateOfBirth = import.Ocse157Verification.DateOfBirth;
    var placeOfBirth = import.Ocse157Verification.PlaceOfBirth ?? "";
    var socialSecurityNumber =
      import.Ocse157Verification.SocialSecurityNumber.GetValueOrDefault();
    var obTranSgi = import.Ocse157Verification.ObTranSgi.GetValueOrDefault();
    var obTranType = import.Ocse157Verification.ObTranType ?? "";
    var obTranAmount =
      import.Ocse157Verification.ObTranAmount.GetValueOrDefault();
    var obligationSgi =
      import.Ocse157Verification.ObligationSgi.GetValueOrDefault();
    var debtAdjType = import.Ocse157Verification.DebtAdjType ?? "";
    var debtDetailDueDt = import.Ocse157Verification.DebtDetailDueDt;
    var debtDetailBalanceDue =
      import.Ocse157Verification.DebtDetailBalanceDue.GetValueOrDefault();
    var obTypeSgi = import.Ocse157Verification.ObTypeSgi.GetValueOrDefault();
    var obTypeClassfctn = import.Ocse157Verification.ObTypeClassfctn ?? "";
    var collectionSgi =
      import.Ocse157Verification.CollectionSgi.GetValueOrDefault();
    var collectionAmount =
      import.Ocse157Verification.CollectionAmount.GetValueOrDefault();
    var collectionDte = import.Ocse157Verification.CollectionDte;
    var collApplToCode = import.Ocse157Verification.CollApplToCode ?? "";
    var collCreatedDte = import.Ocse157Verification.CollCreatedDte;
    var caseRoleType = import.Ocse157Verification.CaseRoleType ?? "";
    var caseWorkerNumber = import.Ocse157Verification.CaseWorkerNumber ?? "";
    var caseWorkerName = import.Ocse157Verification.CaseWorkerName ?? "";
    var caseAsinEffDte = import.Ocse157Verification.CaseAsinEffDte;
    var caseAsinEndDte = import.Ocse157Verification.CaseAsinEndDte;
    var intRequestIdent =
      import.Ocse157Verification.IntRequestIdent.GetValueOrDefault();
    var intRqstRqstDte = import.Ocse157Verification.IntRqstRqstDte;
    var kansasCaseInd = import.Ocse157Verification.KansasCaseInd ?? "";
    var personProgCode = import.Ocse157Verification.PersonProgCode ?? "";
    var hlthInsCovrgId =
      import.Ocse157Verification.HlthInsCovrgId.GetValueOrDefault();
    var goodCauseEffDte = import.Ocse157Verification.GoodCauseEffDte;
    var noCoopEffDte = import.Ocse157Verification.NoCoopEffDte;
    var comment = import.Ocse157Verification.Comment ?? "";
    var child4DigitSsn = import.Ocse157Verification.Child4DigitSsn ?? "";
    var ap4DigitSsn = import.Ocse157Verification.Ap4DigitSsn ?? "";
    var ar4DigitSsn = import.Ocse157Verification.Ar4DigitSsn ?? "";
    var arName = import.Ocse157Verification.ArName ?? "";
    var apName = import.Ocse157Verification.ApName ?? "";
    var childName = import.Ocse157Verification.ChildName ?? "";

    entities.Ocse157Verification.Populated = false;
    Update("CreateOcse157Verification",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fiscalYear", fiscalYear);
        db.SetNullableInt32(command, "runNumber", runNumber);
        db.SetNullableString(command, "lineNumber", lineNumber);
        db.SetNullableString(command, "column0", column);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "suppPersonNumber", suppPersonNumber);
        db.SetNullableString(command, "obligorPersonNbr", obligorPersonNbr);
        db.SetNullableDate(command, "dtePaternityEst", dtePaternityEst);
        db.SetNullableString(command, "courtOrderNbr", courtOrderNumber);
        db.SetNullableDate(command, "legalCreatedDte", legalCreatedDte);
        db.SetNullableDate(command, "dateOfBirth", dateOfBirth);
        db.SetNullableString(command, "placeOfBirth", placeOfBirth);
        db.SetNullableInt32(command, "socialSecurityNb", socialSecurityNumber);
        db.SetNullableInt32(command, "obTranSgi", obTranSgi);
        db.SetNullableString(command, "obTranType", obTranType);
        db.SetNullableDecimal(command, "obTranAmount", obTranAmount);
        db.SetNullableInt32(command, "obligationSgi", obligationSgi);
        db.SetNullableString(command, "debtAdjType", debtAdjType);
        db.SetNullableDate(command, "debtDetailDueDt", debtDetailDueDt);
        db.SetNullableDecimal(command, "debtDtlBalDue", debtDetailBalanceDue);
        db.SetNullableInt32(command, "obTypeSgi", obTypeSgi);
        db.SetNullableString(command, "obTypeClassfctn", obTypeClassfctn);
        db.SetNullableInt32(command, "collectionSgi", collectionSgi);
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableDate(command, "collectionDte", collectionDte);
        db.SetNullableString(command, "collApplToCode", collApplToCode);
        db.SetNullableDate(command, "collCreatedDte", collCreatedDte);
        db.SetNullableString(command, "caseRoleType", caseRoleType);
        db.SetNullableString(command, "caseWorkerNumber", caseWorkerNumber);
        db.SetNullableString(command, "caseWorkerName", caseWorkerName);
        db.SetNullableDate(command, "caseAsinEffDte", caseAsinEffDte);
        db.SetNullableDate(command, "caseAsinEndDte", caseAsinEndDte);
        db.SetNullableInt32(command, "intRequestIdent", intRequestIdent);
        db.SetNullableDate(command, "intRqstRqstDte", intRqstRqstDte);
        db.SetNullableString(command, "kansasCaseInd", kansasCaseInd);
        db.SetNullableString(command, "personProgCode", personProgCode);
        db.SetNullableInt64(command, "hlthInsCovrgId", hlthInsCovrgId);
        db.SetNullableDate(command, "goodCauseEffDte", goodCauseEffDte);
        db.SetNullableDate(command, "noCoopEffDte", noCoopEffDte);
        db.SetNullableString(command, "comment0", comment);
        db.SetNullableString(command, "child4DigitSsn", child4DigitSsn);
        db.SetNullableString(command, "ap4DigitSsn", ap4DigitSsn);
        db.SetNullableString(command, "ar4DigitSsn", ar4DigitSsn);
        db.SetNullableString(command, "arName", arName);
        db.SetNullableString(command, "apName", apName);
        db.SetNullableString(command, "childName", childName);
      });

    entities.Ocse157Verification.FiscalYear = fiscalYear;
    entities.Ocse157Verification.RunNumber = runNumber;
    entities.Ocse157Verification.LineNumber = lineNumber;
    entities.Ocse157Verification.Column = column;
    entities.Ocse157Verification.CreatedTimestamp = createdTimestamp;
    entities.Ocse157Verification.CaseNumber = caseNumber;
    entities.Ocse157Verification.SuppPersonNumber = suppPersonNumber;
    entities.Ocse157Verification.ObligorPersonNbr = obligorPersonNbr;
    entities.Ocse157Verification.DtePaternityEst = dtePaternityEst;
    entities.Ocse157Verification.CourtOrderNumber = courtOrderNumber;
    entities.Ocse157Verification.LegalCreatedDte = legalCreatedDte;
    entities.Ocse157Verification.DateOfBirth = dateOfBirth;
    entities.Ocse157Verification.PlaceOfBirth = placeOfBirth;
    entities.Ocse157Verification.SocialSecurityNumber = socialSecurityNumber;
    entities.Ocse157Verification.ObTranSgi = obTranSgi;
    entities.Ocse157Verification.ObTranType = obTranType;
    entities.Ocse157Verification.ObTranAmount = obTranAmount;
    entities.Ocse157Verification.ObligationSgi = obligationSgi;
    entities.Ocse157Verification.DebtAdjType = debtAdjType;
    entities.Ocse157Verification.DebtDetailDueDt = debtDetailDueDt;
    entities.Ocse157Verification.DebtDetailBalanceDue = debtDetailBalanceDue;
    entities.Ocse157Verification.ObTypeSgi = obTypeSgi;
    entities.Ocse157Verification.ObTypeClassfctn = obTypeClassfctn;
    entities.Ocse157Verification.CollectionSgi = collectionSgi;
    entities.Ocse157Verification.CollectionAmount = collectionAmount;
    entities.Ocse157Verification.CollectionDte = collectionDte;
    entities.Ocse157Verification.CollApplToCode = collApplToCode;
    entities.Ocse157Verification.CollCreatedDte = collCreatedDte;
    entities.Ocse157Verification.CaseRoleType = caseRoleType;
    entities.Ocse157Verification.CaseWorkerNumber = caseWorkerNumber;
    entities.Ocse157Verification.CaseWorkerName = caseWorkerName;
    entities.Ocse157Verification.CaseAsinEffDte = caseAsinEffDte;
    entities.Ocse157Verification.CaseAsinEndDte = caseAsinEndDte;
    entities.Ocse157Verification.IntRequestIdent = intRequestIdent;
    entities.Ocse157Verification.IntRqstRqstDte = intRqstRqstDte;
    entities.Ocse157Verification.KansasCaseInd = kansasCaseInd;
    entities.Ocse157Verification.PersonProgCode = personProgCode;
    entities.Ocse157Verification.HlthInsCovrgId = hlthInsCovrgId;
    entities.Ocse157Verification.GoodCauseEffDte = goodCauseEffDte;
    entities.Ocse157Verification.NoCoopEffDte = noCoopEffDte;
    entities.Ocse157Verification.Comment = comment;
    entities.Ocse157Verification.Child4DigitSsn = child4DigitSsn;
    entities.Ocse157Verification.Ap4DigitSsn = ap4DigitSsn;
    entities.Ocse157Verification.Ar4DigitSsn = ar4DigitSsn;
    entities.Ocse157Verification.ArName = arName;
    entities.Ocse157Verification.ApName = apName;
    entities.Ocse157Verification.ChildName = childName;
    entities.Ocse157Verification.Populated = true;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private Ocse157Verification ocse157Verification;
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
    /// A value of NbrOfTries.
    /// </summary>
    [JsonPropertyName("nbrOfTries")]
    public Common NbrOfTries
    {
      get => nbrOfTries ??= new();
      set => nbrOfTries = value;
    }

    private Common nbrOfTries;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private Ocse157Verification ocse157Verification;
  }
#endregion
}

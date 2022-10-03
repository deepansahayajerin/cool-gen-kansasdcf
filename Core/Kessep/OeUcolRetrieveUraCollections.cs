// Program: OE_UCOL_RETRIEVE_URA_COLLECTIONS, ID: 374449588, model: 746.
// Short name: SWE02880
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_UCOL_RETRIEVE_URA_COLLECTIONS.
/// </summary>
[Serializable]
public partial class OeUcolRetrieveUraCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UCOL_RETRIEVE_URA_COLLECTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUcolRetrieveUraCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUcolRetrieveUraCollections.
  /// </summary>
  public OeUcolRetrieveUraCollections(IContext context, Import import,
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
    export.Obligor.Number = import.Obligor.Number;
    export.Member.Number = import.Member.Number;
    export.ImHousehold.AeCaseNo = import.ImHousehold.AeCaseNo;
    local.ServiceProviderRequired.Flag = "N";

    if (IsEmpty(import.ImHousehold.AeCaseNo))
    {
    }
    else
    {
      local.TextWorkArea.Text10 = import.ImHousehold.AeCaseNo;
      UseEabPadLeftWithZeros2();
      export.ImHousehold.AeCaseNo = Substring(local.TextWorkArea.Text10, 3, 8);

      if (!ReadImHousehold())
      {
        ExitState = "IM_HOUSEHOLD_NF";

        return;
      }
    }

    if (!IsEmpty(import.Member.Number))
    {
      if (ReadCsePerson1())
      {
        UseSiReadCsePerson1();
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    // : Get obligor number if possible.  If obligor number or case or court 
    // order was entered,
    //  the obligor cse person number is available.
    if (IsEmpty(import.Obligor.Number))
    {
      if (IsEmpty(import.Case1.Number))
      {
        if (IsEmpty(import.LegalAction.StandardNumber))
        {
        }
        else if (ReadLegalAction())
        {
          if (ReadCsePersonObligor1())
          {
            local.ObligorCsePersonsWorkSet.Number =
              entities.SearchObligor.Number;
            UseSiReadCsePerson2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.ObligorCsePersonsWorkSet.FormattedName = "no name";
              ExitState = "ACO_NN0000_ALL_OK";
            }

            MoveCsePerson(entities.SearchObligor, local.ObligorCsePerson);
          }
          else
          {
            ExitState = "FN0000_OBLIGOR_CSE_PERSON_NF";

            return;
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }
      }
      else if (ReadCase())
      {
        if (ReadCsePersonObligor2())
        {
          local.ObligorCsePersonsWorkSet.Number = entities.SearchObligor.Number;
          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ObligorCsePersonsWorkSet.FormattedName = "no name";
            ExitState = "ACO_NN0000_ALL_OK";
          }

          MoveCsePerson(entities.SearchObligor, local.ObligorCsePerson);
        }
        else
        {
          ExitState = "FN0000_OBLIGOR_CSE_PERSON_NF";

          return;
        }
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else if (ReadCsePersonObligor3())
    {
      export.Obligor.Number = entities.SearchObligor.Number;
      UseSiReadCsePerson5();
      ExitState = "ACO_NN0000_ALL_OK";
      MoveCsePerson(entities.SearchObligor, local.ObligorCsePerson);
      MoveCsePersonsWorkSet(export.Obligor, local.ObligorCsePersonsWorkSet);
    }
    else
    {
      ExitState = "FN0000_OBLIGOR_CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------------------------
    //                      DATA RETRIEVAL
    // ---------------------------------------------------------------
    export.Export1.Index = -1;

    // -----------------------------------------------------------------------------------
    // This read handles Member number + any other combination of criteria.
    // -----------------------------------------------------------------------------------
    if (entities.Member.Populated)
    {
      foreach(var item in ReadImHouseholdMbrMnthlySumImHousehold())
      {
        local.DateWorkArea.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;

        if (local.DateWorkArea.YearMonth >= import.From.YearMonth && local
          .DateWorkArea.YearMonth <= import.To.YearMonth)
        {
        }
        else
        {
          continue;
        }

        foreach(var item1 in ReadUraCollectionApplicationCollection())
        {
          if (!IsEmpty(import.LegalAction.StandardNumber))
          {
            if (!Equal(import.LegalAction.StandardNumber,
              entities.Collection.CourtOrderAppliedTo))
            {
              continue;
            }
          }

          // : If an obligor has been entered as search criteria, use it to 
          // qualify the read.  Also set the previous obligor number to the
          // search number, as it will be the same for all obligors on the list.
          if (entities.SearchObligor.Populated)
          {
            MoveCsePersonsWorkSet(local.ObligorCsePersonsWorkSet,
              local.PrevObligor);

            if (!ReadDebtCsePersonDebtDetailObligationType2())
            {
              continue;
            }
          }
          else if (ReadDebtCsePersonCsePersonDebtDetailObligationType())
          {
            MoveCsePerson(entities.Obligor2, local.ObligorCsePerson);
          }
          else
          {
            ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

            return;
          }

          if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          MoveCollection(entities.Collection, export.Export1.Update.Collection);
          MoveUraCollectionApplication(entities.UraCollectionApplication,
            export.Export1.Update.UraCollectionApplication);
          MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
            export.Export1.Update.ImHouseholdMbrMnthlySum);
          export.Export1.Update.Supported.Number = entities.Supported2.Number;
          export.Export1.Update.Obligor.Number = entities.Obligor2.Number;
          export.Export1.Update.ImHousehold.AeCaseNo =
            entities.ImHousehold.AeCaseNo;

          if (ReadCollectionType())
          {
            export.Export1.Update.CollectionType.Code =
              entities.CollectionType.Code;
          }
          else
          {
            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }

          UseFnDetCaseNoAndWrkrForDbt();
          export.Export1.Update.Case1.Number = local.Case1.Number;

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (!IsEmpty(import.Case1.Number) && !
            Equal(local.Case1.Number, import.Case1.Number))
          {
            continue;
          }

          if (!Equal(entities.Obligor2.Number, local.PrevObligor.Number))
          {
            local.PrevObligor.Number = entities.Obligor2.Number;
            UseSiReadCsePerson4();
            MoveCsePersonsWorkSet(local.PrevObligor,
              export.Export1.Update.Obligor);
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            MoveCsePersonsWorkSet(local.PrevObligor,
              export.Export1.Update.Obligor);
          }

          MoveCsePersonsWorkSet(export.Member, export.Export1.Update.Member);

          // : Get Supported person number and formatted name
          local.CsePersonNumber.Text10 = entities.Supported2.Number;
          UseEabPadLeftWithZeros1();
          local.Work.Number = local.CsePersonNumber.Text10;
          UseSiReadCsePerson3();
          MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Supported);
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      // : End of data retrieval for any combination of criteria that
      //   included member CSE person number.
      return;
    }

    // -----------------------------------------------------------------------------------
    // AE Case Number + any combination of Obligor, Legal Action and CSE Case 
    // Number.
    // -----------------------------------------------------------------------------------
    if (!IsEmpty(import.ImHousehold.AeCaseNo))
    {
      foreach(var item in ReadImHouseholdMbrMnthlySumCsePerson())
      {
        local.DateWorkArea.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;

        if (local.DateWorkArea.YearMonth >= import.From.YearMonth && local
          .DateWorkArea.YearMonth <= import.To.YearMonth)
        {
        }
        else
        {
          continue;
        }

        foreach(var item1 in ReadUraCollectionApplicationCollection())
        {
          if (!IsEmpty(import.LegalAction.StandardNumber))
          {
            if (!Equal(import.LegalAction.StandardNumber,
              entities.Collection.CourtOrderAppliedTo))
            {
              continue;
            }
          }

          // : If obligor was entered as part of search criteria, use it in
          //   qualifying the read, otherwise, read all obligors.
          if (entities.SearchObligor.Populated)
          {
            if (!ReadDebtCsePersonDebtDetailObligationType1())
            {
              continue;
            }
          }
          else if (ReadDebtCsePersonCsePersonDebtDetailObligationType())
          {
            MoveCsePerson(entities.Obligor2, local.ObligorCsePerson);
          }
          else
          {
            ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

            return;
          }

          if (!ReadCollectionType())
          {
            ExitState = "FN0000_COLLECTION_TYPE_NF";

            return;
          }

          UseFnDetCaseNoAndWrkrForDbt();

          if (!IsEmpty(import.Case1.Number) && !
            Equal(local.Case1.Number, import.Case1.Number))
          {
            continue;
          }

          if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
          {
            ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

            return;
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.ImHousehold.AeCaseNo =
            entities.ImHousehold.AeCaseNo;
          export.Export1.Update.Member.Number = entities.Member.Number;
          export.Export1.Update.Supported.Number = entities.Supported2.Number;
          export.Export1.Update.Obligor.Number = entities.Obligor2.Number;
          MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
            export.Export1.Update.ImHouseholdMbrMnthlySum);
          MoveCollection(entities.Collection, export.Export1.Update.Collection);
          MoveUraCollectionApplication(entities.UraCollectionApplication,
            export.Export1.Update.UraCollectionApplication);
          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
          export.Export1.Update.Case1.Number = local.Case1.Number;

          if (!IsEmpty(local.ObligorCsePersonsWorkSet.FormattedName))
          {
            // : The obligor was entered as part of search criteria, so it will
            //   be the same throughout the list.  Just copy the search number
            //   and name into the group view.
            MoveCsePersonsWorkSet(local.ObligorCsePersonsWorkSet,
              export.Export1.Update.Obligor);
          }
          else
          {
            local.Work.Number = entities.Obligor2.Number;
            UseSiReadCsePerson3();
            MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Obligor);
            ExitState = "ACO_NN0000_ALL_OK";
          }

          // : Get Member formatted name
          local.Work.Number = entities.Member.Number;
          UseSiReadCsePerson3();
          MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Member);

          // : Get Supported person formatted name
          local.Work.Number = entities.Supported2.Number;
          UseSiReadCsePerson3();
          MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Supported);
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      return;
    }

    // -----------------------------------------------------------------------------------
    // This read is for when the following search criteria is entered:
    // OBLIGOR
    // OBLIGOR + CASE
    // OBLIGOR + COURT ORDER
    // OBLIGOR + CASE + COURT ORDER
    // CASE
    // CASE + COURT ORDER
    // COURT ORDER
    // -----------------------------------------------------------------------------------
    if (!IsEmpty(import.Obligor.Number) || entities.SearchObligor.Populated)
    {
      foreach(var item in ReadUraCollectionApplicationCollectionImHouseholdMbrMnthlySum())
        
      {
        local.DateWorkArea.YearMonth = entities.ImHouseholdMbrMnthlySum.Year * 100
          + entities.ImHouseholdMbrMnthlySum.Month;

        if (local.DateWorkArea.YearMonth >= import.From.YearMonth && local
          .DateWorkArea.YearMonth <= import.To.YearMonth)
        {
        }
        else
        {
          continue;
        }

        if (!IsEmpty(import.LegalAction.StandardNumber))
        {
          if (!Equal(import.LegalAction.StandardNumber,
            entities.Collection.CourtOrderAppliedTo))
          {
            continue;
          }
        }

        if (!ReadDebtCsePersonDebtDetailObligationType3())
        {
          continue;
        }

        if (!ReadCollectionType())
        {
          ExitState = "FN0000_COLLECTION_TYPE_NF";

          return;
        }

        UseFnDetCaseNoAndWrkrForDbt();

        if (!IsEmpty(import.Case1.Number) && !
          Equal(local.Case1.Number, import.Case1.Number))
        {
          continue;
        }

        if (!ReadCsePerson2())
        {
          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.ImHousehold.AeCaseNo =
          entities.ImHousehold.AeCaseNo;
        export.Export1.Update.Member.Number = entities.Member.Number;
        export.Export1.Update.Supported.Number = entities.Supported2.Number;
        export.Export1.Update.Obligor.Number = entities.Obligor2.Number;
        MoveImHouseholdMbrMnthlySum(entities.ImHouseholdMbrMnthlySum,
          export.Export1.Update.ImHouseholdMbrMnthlySum);
        MoveCollection(entities.Collection, export.Export1.Update.Collection);
        MoveUraCollectionApplication(entities.UraCollectionApplication,
          export.Export1.Update.UraCollectionApplication);
        export.Export1.Update.CollectionType.Code =
          entities.CollectionType.Code;
        export.Export1.Update.Case1.Number = local.Case1.Number;

        // : The obligor was entered as part of search criteria, so it will
        //   be the same throughout the list.  Just copy the search number
        //   and name into the group view.
        MoveCsePersonsWorkSet(local.ObligorCsePersonsWorkSet,
          export.Export1.Update.Obligor);

        // : Get Member formatted name
        local.Work.Number = entities.Member.Number;
        UseSiReadCsePerson3();
        MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Member);

        // : Get Supported person formatted name
        local.Work.Number = entities.Supported2.Number;
        UseSiReadCsePerson3();
        MoveCsePersonsWorkSet(local.Work, export.Export1.Update.Supported);
        ExitState = "ACO_NN0000_ALL_OK";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.DistributionMethod = source.DistributionMethod;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveImHouseholdMbrMnthlySum(
    ImHouseholdMbrMnthlySum source, ImHouseholdMbrMnthlySum target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
  }

  private static void MoveUraCollectionApplication(
    UraCollectionApplication source, UraCollectionApplication target)
  {
    target.CollectionAmountApplied = source.CollectionAmountApplied;
    target.Type1 = source.Type1;
  }

  private void UseEabPadLeftWithZeros1()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.CsePersonNumber.Text10;
    useExport.TextWorkArea.Text10 = local.CsePersonNumber.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.CsePersonNumber.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabPadLeftWithZeros2()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnDetCaseNoAndWrkrForDbt()
  {
    var useImport = new FnDetCaseNoAndWrkrForDbt.Import();
    var useExport = new FnDetCaseNoAndWrkrForDbt.Export();

    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    useImport.Supported.Number = entities.Supported2.Number;
    useImport.Obligor.Number = local.ObligorCsePerson.Number;

    Call(FnDetCaseNoAndWrkrForDbt.Execute, useImport, useExport);

    local.Case1.Number = useExport.Case1.Number;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.Member.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Member);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Work.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.Work);
  }

  private void UseSiReadCsePerson4()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.PrevObligor.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.PrevObligor);
  }

  private void UseSiReadCsePerson5()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Obligor.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Obligor);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Member.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Member.Number);
      },
      (db, reader) =>
      {
        entities.Member.Number = db.GetString(reader, 0);
        entities.Member.Type1 = db.GetString(reader, 1);
        entities.Member.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.Member.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.
          SetString(command, "numb", entities.ImHouseholdMbrMnthlySum.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.Member.Number = db.GetString(reader, 0);
        entities.Member.Type1 = db.GetString(reader, 1);
        entities.Member.Populated = true;
      });
  }

  private bool ReadCsePersonObligor1()
  {
    entities.Obligor1.Populated = false;
    entities.SearchObligor.Populated = false;

    return Read("ReadCsePersonObligor1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SearchObligor.Number = db.GetString(reader, 0);
        entities.SearchObligor.Type1 = db.GetString(reader, 1);
        entities.Obligor1.CspNumber = db.GetString(reader, 2);
        entities.Obligor1.Type1 = db.GetString(reader, 3);
        entities.Obligor1.Populated = true;
        entities.SearchObligor.Populated = true;
      });
  }

  private bool ReadCsePersonObligor2()
  {
    entities.Obligor1.Populated = false;
    entities.SearchObligor.Populated = false;

    return Read("ReadCsePersonObligor2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.SearchObligor.Number = db.GetString(reader, 0);
        entities.Obligor1.CspNumber = db.GetString(reader, 0);
        entities.SearchObligor.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Type1 = db.GetString(reader, 2);
        entities.Obligor1.Populated = true;
        entities.SearchObligor.Populated = true;
      });
  }

  private bool ReadCsePersonObligor3()
  {
    entities.Obligor1.Populated = false;
    entities.SearchObligor.Populated = false;

    return Read("ReadCsePersonObligor3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.SearchObligor.Number = db.GetString(reader, 0);
        entities.Obligor1.CspNumber = db.GetString(reader, 0);
        entities.SearchObligor.Type1 = db.GetString(reader, 1);
        entities.Obligor1.Type1 = db.GetString(reader, 2);
        entities.Obligor1.Populated = true;
        entities.SearchObligor.Populated = true;
      });
  }

  private bool ReadDebtCsePersonCsePersonDebtDetailObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Supported2.Populated = false;
    entities.Obligor2.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePersonCsePersonDebtDetailObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Obligor2.Number = db.GetString(reader, 8);
        entities.Obligor2.Type1 = db.GetString(reader, 9);
        entities.Supported2.Number = db.GetString(reader, 10);
        entities.Supported2.Type1 = db.GetString(reader, 11);
        entities.DebtDetail.DueDt = db.GetDate(reader, 12);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.ObligationType.Code = db.GetString(reader, 14);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 15);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Supported2.Populated = true;
        entities.Obligor2.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePersonDebtDetailObligationType1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Supported2.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePersonDebtDetailObligationType1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType1", entities.Collection.CpaType);
        db.SetString(command, "cspNumber1", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
        db.SetString(command, "cpaType2", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber2", entities.Obligor1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Supported2.Number = db.GetString(reader, 8);
        entities.Supported2.Type1 = db.GetString(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 13);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Supported2.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePersonDebtDetailObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Supported2.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePersonDebtDetailObligationType2",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType1", entities.Collection.CpaType);
        db.SetString(command, "cspNumber1", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
        db.SetString(command, "cpaType2", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber2", entities.Obligor1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Supported2.Number = db.GetString(reader, 8);
        entities.Supported2.Type1 = db.GetString(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 13);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Supported2.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadDebtCsePersonDebtDetailObligationType3()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationType.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Supported2.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtCsePersonDebtDetailObligationType3",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Supported2.Number = db.GetString(reader, 8);
        entities.Supported2.Type1 = db.GetString(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 13);
        entities.ObligationType.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Supported2.Populated = true;
        entities.Debt.Populated = true;
      });
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", export.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumCsePerson()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.Member.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumCsePerson",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetInt32(command, "year1", import.From.Year);
        db.SetInt32(command, "year2", import.To.Year);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 4);
        entities.Member.Number = db.GetString(reader, 4);
        entities.Member.Type1 = db.GetString(reader, 5);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.Member.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadImHouseholdMbrMnthlySumImHousehold()
  {
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.ImHousehold.Populated = false;

    return ReadEach("ReadImHouseholdMbrMnthlySumImHousehold",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Member.Number);
        db.SetInt32(command, "year1", import.From.Year);
        db.SetInt32(command, "year2", import.To.Year);
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 0);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 1);
        entities.ImHouseholdMbrMnthlySum.Relationship = db.GetString(reader, 2);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 3);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 3);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 4);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ImHousehold.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadUraCollectionApplicationCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMbrMnthlySum.Populated);
    entities.Collection.Populated = false;
    entities.UraCollectionApplication.Populated = false;

    return ReadEach("ReadUraCollectionApplicationCollection",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber0", entities.ImHouseholdMbrMnthlySum.CspNumber);
        db.SetInt32(command, "imsYear", entities.ImHouseholdMbrMnthlySum.Year);
        db.
          SetInt32(command, "imsMonth", entities.ImHouseholdMbrMnthlySum.Month);
          
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo);
          
        db.SetDate(
          command, "date1", import.SearchCollFrom.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.SearchCollTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CollectionAmountApplied =
          db.GetDecimal(reader, 0);
        entities.UraCollectionApplication.CreatedBy = db.GetString(reader, 1);
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 2);
        entities.Collection.CspNumber = db.GetString(reader, 2);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 3);
        entities.Collection.CpaType = db.GetString(reader, 3);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.OtyId = db.GetInt32(reader, 4);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 6);
        entities.Collection.OtrId = db.GetInt32(reader, 6);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 7);
        entities.Collection.OtrType = db.GetString(reader, 7);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 9);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 10);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 11);
        entities.Collection.CrdId = db.GetInt32(reader, 11);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 12);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 13);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 14);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 15);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 16);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 17);
        entities.UraCollectionApplication.Type1 =
          db.GetNullableString(reader, 18);
        entities.Collection.AppliedToCode = db.GetString(reader, 19);
        entities.Collection.CollectionDt = db.GetDate(reader, 20);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 21);
        entities.Collection.ConcurrentInd = db.GetString(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DistributionMethod = db.GetString(reader, 24);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 25);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 26);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 27);
        entities.Collection.Populated = true;
        entities.UraCollectionApplication.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadUraCollectionApplicationCollectionImHouseholdMbrMnthlySum()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor1.Populated);
    entities.ImHouseholdMbrMnthlySum.Populated = false;
    entities.ImHousehold.Populated = false;
    entities.Collection.Populated = false;
    entities.UraCollectionApplication.Populated = false;

    return ReadEach(
      "ReadUraCollectionApplicationCollectionImHouseholdMbrMnthlySum",
      (db, command) =>
      {
        db.SetInt32(command, "year1", import.From.Year);
        db.SetInt32(command, "year2", import.To.Year);
        db.SetString(command, "cpaType", entities.Obligor1.Type1);
        db.SetString(command, "cspNumber", entities.Obligor1.CspNumber);
        db.SetDate(
          command, "date1", import.SearchCollFrom.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", import.SearchCollTo.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.UraCollectionApplication.CollectionAmountApplied =
          db.GetDecimal(reader, 0);
        entities.UraCollectionApplication.CreatedBy = db.GetString(reader, 1);
        entities.UraCollectionApplication.CspNumber = db.GetString(reader, 2);
        entities.Collection.CspNumber = db.GetString(reader, 2);
        entities.UraCollectionApplication.CpaType = db.GetString(reader, 3);
        entities.Collection.CpaType = db.GetString(reader, 3);
        entities.UraCollectionApplication.OtyIdentifier =
          db.GetInt32(reader, 4);
        entities.Collection.OtyId = db.GetInt32(reader, 4);
        entities.UraCollectionApplication.ObgIdentifier =
          db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.UraCollectionApplication.OtrIdentifier =
          db.GetInt32(reader, 6);
        entities.Collection.OtrId = db.GetInt32(reader, 6);
        entities.UraCollectionApplication.OtrType = db.GetString(reader, 7);
        entities.Collection.OtrType = db.GetString(reader, 7);
        entities.UraCollectionApplication.CstIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.UraCollectionApplication.CrvIdentifier =
          db.GetInt32(reader, 9);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.UraCollectionApplication.CrtIdentifier =
          db.GetInt32(reader, 10);
        entities.Collection.CrtType = db.GetInt32(reader, 10);
        entities.UraCollectionApplication.CrdIdentifier =
          db.GetInt32(reader, 11);
        entities.Collection.CrdId = db.GetInt32(reader, 11);
        entities.UraCollectionApplication.ColIdentifier =
          db.GetInt32(reader, 12);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.UraCollectionApplication.ImhAeCaseNo =
          db.GetString(reader, 13);
        entities.ImHouseholdMbrMnthlySum.ImhAeCaseNo = db.GetString(reader, 13);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 13);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 13);
        entities.UraCollectionApplication.CspNumber0 = db.GetString(reader, 14);
        entities.ImHouseholdMbrMnthlySum.CspNumber = db.GetString(reader, 14);
        entities.UraCollectionApplication.ImsMonth = db.GetInt32(reader, 15);
        entities.ImHouseholdMbrMnthlySum.Month = db.GetInt32(reader, 15);
        entities.UraCollectionApplication.ImsYear = db.GetInt32(reader, 16);
        entities.ImHouseholdMbrMnthlySum.Year = db.GetInt32(reader, 16);
        entities.UraCollectionApplication.CreatedTstamp =
          db.GetDateTime(reader, 17);
        entities.UraCollectionApplication.Type1 =
          db.GetNullableString(reader, 18);
        entities.Collection.AppliedToCode = db.GetString(reader, 19);
        entities.Collection.CollectionDt = db.GetDate(reader, 20);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 21);
        entities.Collection.ConcurrentInd = db.GetString(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DistributionMethod = db.GetString(reader, 24);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 25);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 26);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 27);
        entities.ImHouseholdMbrMnthlySum.Relationship =
          db.GetString(reader, 28);
        entities.ImHouseholdMbrMnthlySum.Populated = true;
        entities.ImHousehold.Populated = true;
        entities.Collection.Populated = true;
        entities.UraCollectionApplication.Populated = true;

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
    /// A value of SearchCollFrom.
    /// </summary>
    [JsonPropertyName("searchCollFrom")]
    public DateWorkArea SearchCollFrom
    {
      get => searchCollFrom ??= new();
      set => searchCollFrom = value;
    }

    /// <summary>
    /// A value of SearchCollTo.
    /// </summary>
    [JsonPropertyName("searchCollTo")]
    public DateWorkArea SearchCollTo
    {
      get => searchCollTo ??= new();
      set => searchCollTo = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePersonsWorkSet Member
    {
      get => member ??= new();
      set => member = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ToMonthyear.
    /// </summary>
    [JsonPropertyName("toMonthyear")]
    public DateWorkArea ToMonthyear
    {
      get => toMonthyear ??= new();
      set => toMonthyear = value;
    }

    /// <summary>
    /// A value of FromMonthyear.
    /// </summary>
    [JsonPropertyName("fromMonthyear")]
    public DateWorkArea FromMonthyear
    {
      get => fromMonthyear ??= new();
      set => fromMonthyear = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    private DateWorkArea searchCollFrom;
    private DateWorkArea searchCollTo;
    private CsePersonsWorkSet obligor;
    private CsePersonsWorkSet member;
    private ImHousehold imHousehold;
    private LegalAction legalAction;
    private Case1 case1;
    private DateWorkArea toMonthyear;
    private DateWorkArea fromMonthyear;
    private DateWorkArea to;
    private DateWorkArea from;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>
      /// A value of UraCollectionApplication.
      /// </summary>
      [JsonPropertyName("uraCollectionApplication")]
      public UraCollectionApplication UraCollectionApplication
      {
        get => uraCollectionApplication ??= new();
        set => uraCollectionApplication = value;
      }

      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>
      /// A value of ImHousehold.
      /// </summary>
      [JsonPropertyName("imHousehold")]
      public ImHousehold ImHousehold
      {
        get => imHousehold ??= new();
        set => imHousehold = value;
      }

      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of Obligor.
      /// </summary>
      [JsonPropertyName("obligor")]
      public CsePersonsWorkSet Obligor
      {
        get => obligor ??= new();
        set => obligor = value;
      }

      /// <summary>
      /// A value of Member.
      /// </summary>
      [JsonPropertyName("member")]
      public CsePersonsWorkSet Member
      {
        get => member ??= new();
        set => member = value;
      }

      /// <summary>
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
      }

      /// <summary>
      /// A value of ImHouseholdMbrMnthlySum.
      /// </summary>
      [JsonPropertyName("imHouseholdMbrMnthlySum")]
      public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
      {
        get => imHouseholdMbrMnthlySum ??= new();
        set => imHouseholdMbrMnthlySum = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 70;

      private Common common;
      private Collection collection;
      private UraCollectionApplication uraCollectionApplication;
      private CollectionType collectionType;
      private ImHousehold imHousehold;
      private Case1 case1;
      private CsePersonsWorkSet obligor;
      private CsePersonsWorkSet member;
      private CsePersonsWorkSet supported;
      private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePersonsWorkSet Member
    {
      get => member ??= new();
      set => member = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private CsePersonsWorkSet obligor;
    private CsePersonsWorkSet member;
    private ImHousehold imHousehold;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of TestMemberCount.
    /// </summary>
    [JsonPropertyName("testMemberCount")]
    public Common TestMemberCount
    {
      get => testMemberCount ??= new();
      set => testMemberCount = value;
    }

    /// <summary>
    /// A value of ServiceProviderRequired.
    /// </summary>
    [JsonPropertyName("serviceProviderRequired")]
    public Common ServiceProviderRequired
    {
      get => serviceProviderRequired ??= new();
      set => serviceProviderRequired = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of PrevObligor.
    /// </summary>
    [JsonPropertyName("prevObligor")]
    public CsePersonsWorkSet PrevObligor
    {
      get => prevObligor ??= new();
      set => prevObligor = value;
    }

    /// <summary>
    /// A value of CsePersonNumber.
    /// </summary>
    [JsonPropertyName("csePersonNumber")]
    public TextWorkArea CsePersonNumber
    {
      get => csePersonNumber ??= new();
      set => csePersonNumber = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson obligorCsePerson;
    private Case1 case1;
    private Common testMemberCount;
    private Common serviceProviderRequired;
    private CsePersonsWorkSet work;
    private CsePersonsWorkSet prevObligor;
    private TextWorkArea csePersonNumber;
    private DateWorkArea dateWorkArea;
    private DateWorkArea null1;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of SearchObligor.
    /// </summary>
    [JsonPropertyName("searchObligor")]
    public CsePerson SearchObligor
    {
      get => searchObligor ??= new();
      set => searchObligor = value;
    }

    /// <summary>
    /// A value of Member.
    /// </summary>
    [JsonPropertyName("member")]
    public CsePerson Member
    {
      get => member ??= new();
      set => member = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of UraCollectionApplication.
    /// </summary>
    [JsonPropertyName("uraCollectionApplication")]
    public UraCollectionApplication UraCollectionApplication
    {
      get => uraCollectionApplication ??= new();
      set => uraCollectionApplication = value;
    }

    private CaseRole caseRole;
    private Case1 case1;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private LegalAction legalAction;
    private Obligation obligation;
    private CsePersonAccount obligor1;
    private CsePersonAccount supported1;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private ImHousehold imHousehold;
    private CsePerson supported2;
    private CsePerson obligor2;
    private CsePerson searchObligor;
    private CsePerson member;
    private Collection collection;
    private CollectionType collectionType;
    private ObligationTransaction debt;
    private CashReceiptDetail cashReceiptDetail;
    private UraCollectionApplication uraCollectionApplication;
  }
#endregion
}

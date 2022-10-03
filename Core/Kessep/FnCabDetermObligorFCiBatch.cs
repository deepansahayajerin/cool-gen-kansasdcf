// Program: FN_CAB_DETERM_OBLIGOR_F_CI_BATCH, ID: 372566797, model: 746.
// Short name: SWE02206
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_DETERM_OBLIGOR_F_CI_BATCH.
/// </para>
/// <para>
/// RESP: FINANCE
/// This common action block returns the obligor for the cash receipt detail 
/// using the court order number and multi payor indicator
/// </para>
/// </summary>
[Serializable]
public partial class FnCabDetermObligorFCiBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_DETERM_OBLIGOR_F_CI_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabDetermObligorFCiBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabDetermObligorFCiBatch.
  /// </summary>
  public FnCabDetermObligorFCiBatch(IContext context, Import import,
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
    // -----------------------------------------------------------------------
    // This action block determines the Obligor using the imported
    // court order number.
    // -----------------------------------------------------------------------
    // Date	By	Description
    // -----------------------------------------------------------------------
    // 021998	govind	Initial code
    // 022498	govind	Modified the exit states returned by CAB MATCH CSE
    // 		PERSON BATCH
    // 040999	JLK	Copied original CAB and customized logic for batch court 
    // interface.  Original CAB included logic to find obligor using person
    // number or using the SSN.  Additional logic was only used by back up and
    // test procedures that will not be moved to production.
    // -----------------------------------------------------------------------
    MoveCashReceiptDetail(import.CashReceiptDetail, export.CashReceiptDetail);

    // -----------------------------------------------------------------------
    // If court order number is supplied, determine the obligor(s) for the court
    // order number and validate against the SSN and/or Obligor person number
    // if given.
    // -----------------------------------------------------------------------
    if (!IsEmpty(export.CashReceiptDetail.CourtOrderNumber))
    {
      UseFnAbObligorListFCtordBatch();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.NoOfObligors.Count == 0)
      {
        ExitState = "FN0000_CRD_OBLIGOR_UNKWN_4_CTORD";

        return;
      }

      if (local.NoOfObligors.Count == 1)
      {
        // ---------------------------------------------------------------
        // Court order has only one obligor.
        // Populate the obligor's SSN and name information.
        // ---------------------------------------------------------------
        for(local.ObligorsList.Index = 0; local.ObligorsList.Index < local
          .ObligorsList.Count; ++local.ObligorsList.Index)
        {
          export.CashReceiptDetail.ObligorPersonNumber =
            local.ObligorsList.Item.DetailObligorsListCsePerson.Number;
          export.CashReceiptDetail.ObligorSocialSecurityNumber =
            local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.Ssn;
          export.CashReceiptDetail.ObligorLastName =
            local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
              LastName;
          export.CashReceiptDetail.ObligorFirstName =
            local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
              FirstName;
          export.CashReceiptDetail.ObligorMiddleName =
            local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
              MiddleInitial;

          return;
        }
      }
      else
      {
        // -----------------------------------------------------------------
        // Court order has multiple obligors.  Determine which obligor
        // is correct using the SSN or Multi Payor Indicator.
        // -----------------------------------------------------------------
        switch(AsChar(export.CashReceiptDetail.MultiPayor))
        {
          case ' ':
            // -------------------------------------------------------------
            // No multi payor indicator provided.  If SSN provided,
            // determine which payor matches the SSN.
            // -------------------------------------------------------------
            if (!IsEmpty(import.CashReceiptDetail.ObligorSocialSecurityNumber) &&
              Verify
              (import.CashReceiptDetail.ObligorSocialSecurityNumber, " 0") > 0)
            {
              // ----------------------------------------------------------
              // SSN was specified. Derive the Multipayor indicator
              // ----------------------------------------------------------
              for(local.ObligorsList.Index = 0; local.ObligorsList.Index < local
                .ObligorsList.Count; ++local.ObligorsList.Index)
              {
                if (Equal(local.ObligorsList.Item.
                  DetailObligorsListCsePersonsWorkSet.Ssn,
                  import.CashReceiptDetail.ObligorSocialSecurityNumber))
                {
                  switch(AsChar(local.ObligorsList.Item.
                    DetailObligorsListCsePersonsWorkSet.Sex))
                  {
                    case 'F':
                      // --- Sex : Female (F) --> Multipayor Mother (M)
                      export.CashReceiptDetail.MultiPayor = "M";
                      export.CashReceiptDetail.ObligorPersonNumber =
                        local.ObligorsList.Item.DetailObligorsListCsePerson.
                          Number;
                      export.CashReceiptDetail.ObligorSocialSecurityNumber =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.Ssn;
                      export.CashReceiptDetail.ObligorLastName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.LastName;
                      export.CashReceiptDetail.ObligorFirstName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.FirstName;
                      export.CashReceiptDetail.ObligorMiddleName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.MiddleInitial;

                      return;
                    case 'M':
                      // --- Sex : Male (M) --> Multipayor Father (F)
                      export.CashReceiptDetail.MultiPayor = "F";
                      export.CashReceiptDetail.ObligorPersonNumber =
                        local.ObligorsList.Item.DetailObligorsListCsePerson.
                          Number;
                      export.CashReceiptDetail.ObligorSocialSecurityNumber =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.Ssn;
                      export.CashReceiptDetail.ObligorLastName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.LastName;
                      export.CashReceiptDetail.ObligorFirstName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.FirstName;
                      export.CashReceiptDetail.ObligorMiddleName =
                        local.ObligorsList.Item.
                          DetailObligorsListCsePersonsWorkSet.MiddleInitial;

                      return;
                    default:
                      break;
                  }
                }
              }

              export.CashReceiptDetail.Assign(local.Clear);
              MoveCashReceiptDetail(import.CashReceiptDetail,
                export.CashReceiptDetail);
              ExitState = "FN0000_MIS_OBLR_SSN_FOR_CT_ORD";

              return;
            }
            else
            {
              ExitState = "FN0000_MULT_PAYR_IND_REQD";

              return;
            }

            break;
          case 'F':
            break;
          case 'M':
            break;
          default:
            ExitState = "FN0000_INV_MULTI_PAYOR_IND_N_CRD";

            return;
        }

        // -----------------------------------------------------------------
        // Use the Multi Payor Indicator provided to retrieve obligor
        // information.
        // -----------------------------------------------------------------
        for(local.ObligorsList.Index = 0; local.ObligorsList.Index < local
          .ObligorsList.Count; ++local.ObligorsList.Index)
        {
          switch(AsChar(export.CashReceiptDetail.MultiPayor))
          {
            case 'M':
              // --- If multi-payor is Mother (M) and sex is Female (F): select 
              // that person.
              if (AsChar(local.ObligorsList.Item.
                DetailObligorsListCsePersonsWorkSet.Sex) == 'F')
              {
                export.CashReceiptDetail.ObligorPersonNumber =
                  local.ObligorsList.Item.DetailObligorsListCsePerson.Number;
                export.CashReceiptDetail.ObligorSocialSecurityNumber =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    Ssn;
                export.CashReceiptDetail.ObligorLastName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    LastName;
                export.CashReceiptDetail.ObligorFirstName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    FirstName;
                export.CashReceiptDetail.ObligorMiddleName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    MiddleInitial;

                goto Test;
              }

              break;
            case 'F':
              // --- If multi-payor is Father (F) and sex is Male (M): select 
              // that person.
              if (AsChar(local.ObligorsList.Item.
                DetailObligorsListCsePersonsWorkSet.Sex) == 'M')
              {
                export.CashReceiptDetail.ObligorPersonNumber =
                  local.ObligorsList.Item.DetailObligorsListCsePerson.Number;
                export.CashReceiptDetail.ObligorSocialSecurityNumber =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    Ssn;
                export.CashReceiptDetail.ObligorLastName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    LastName;
                export.CashReceiptDetail.ObligorFirstName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    FirstName;
                export.CashReceiptDetail.ObligorMiddleName =
                  local.ObligorsList.Item.DetailObligorsListCsePersonsWorkSet.
                    MiddleInitial;

                goto Test;
              }

              break;
            default:
              break;
          }
        }
      }

Test:

      // ------------------------------------------------------------------
      // If both the SSN and multipayor indicator were supplied, compare the
      // SSN provided as import to the derived person number/ SSN on
      // the court order.
      // ------------------------------------------------------------------
      if (!IsEmpty(import.CashReceiptDetail.ObligorSocialSecurityNumber) && Verify
        (import.CashReceiptDetail.ObligorSocialSecurityNumber, " 0") > 0)
      {
        if (!Equal(export.CashReceiptDetail.ObligorSocialSecurityNumber,
          import.CashReceiptDetail.ObligorSocialSecurityNumber))
        {
          export.CashReceiptDetail.Assign(local.Clear);
          MoveCashReceiptDetail(import.CashReceiptDetail,
            export.CashReceiptDetail);
          ExitState = "FN0000_MIS_OBLR_SSN_FOR_CT_ORD";
        }
      }
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.MultiPayor = source.MultiPayor;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveObligorListToObligorsList(FnAbObligorListFCtordBatch.
    Export.ObligorListGroup source, Local.ObligorsListGroup target)
  {
    MoveCsePerson(source.DetailObligor, target.DetailObligorsListCsePerson);
    target.DetailObligorsListCsePersonsWorkSet.Assign(source.Detail);
  }

  private void UseFnAbObligorListFCtordBatch()
  {
    var useImport = new FnAbObligorListFCtordBatch.Import();
    var useExport = new FnAbObligorListFCtordBatch.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      export.CashReceiptDetail.CourtOrderNumber;

    Call(FnAbObligorListFCtordBatch.Execute, useImport, useExport);

    local.NoOfObligors.Count = useExport.WorkNoOfObligors.Count;
    useExport.ObligorList.CopyTo(
      local.ObligorsList, MoveObligorListToObligorsList);
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
    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ObligorsListGroup group.</summary>
    [Serializable]
    public class ObligorsListGroup
    {
      /// <summary>
      /// A value of DetailObligorsListCsePerson.
      /// </summary>
      [JsonPropertyName("detailObligorsListCsePerson")]
      public CsePerson DetailObligorsListCsePerson
      {
        get => detailObligorsListCsePerson ??= new();
        set => detailObligorsListCsePerson = value;
      }

      /// <summary>
      /// A value of DetailObligorsListCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailObligorsListCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailObligorsListCsePersonsWorkSet
      {
        get => detailObligorsListCsePersonsWorkSet ??= new();
        set => detailObligorsListCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePerson detailObligorsListCsePerson;
      private CsePersonsWorkSet detailObligorsListCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of NoOfObligors.
    /// </summary>
    [JsonPropertyName("noOfObligors")]
    public Common NoOfObligors
    {
      get => noOfObligors ??= new();
      set => noOfObligors = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CashReceiptDetail Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// Gets a value of ObligorsList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorsListGroup> ObligorsList => obligorsList ??= new(
      ObligorsListGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligorsList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorsList")]
    [Computed]
    public IList<ObligorsListGroup> ObligorsList_Json
    {
      get => obligorsList;
      set => ObligorsList.Assign(value);
    }

    private Common noOfObligors;
    private CashReceiptDetail clear;
    private Array<ObligorsListGroup> obligorsList;
  }
#endregion
}

// Program: CAB_INCREMENT_CO_ARRAY, ID: 372819897, model: 746.
// Short name: SWEFC750
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_INCREMENT_CO_ARRAY.
/// </summary>
[Serializable]
public partial class CabIncrementCoArray: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_INCREMENT_CO_ARRAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabIncrementCoArray(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabIncrementCoArray.
  /// </summary>
  public CabIncrementCoArray(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    import.AcrossCo.Item.DownCo.Index = 0;
    import.AcrossCo.Item.DownCo.CheckSize();

    while(import.AcrossCo.Item.DownCo.Index < Import.DownCoGroup.Capacity)
    {
      import.AcrossCo.Index = 0;
      import.AcrossCo.CheckSize();

      while(import.AcrossCo.Index < Import.AcrossCoGroup.Capacity)
      {
        export.AcrossCo.Index = import.AcrossCo.Index;
        export.AcrossCo.CheckSize();

        export.AcrossCo.Item.DownCo.Index = import.AcrossCo.Item.DownCo.Index;
        export.AcrossCo.Item.DownCo.CheckSize();

        MoveCollectionsExtract(import.AcrossCo.Item.DownCo.Item.
          DtlCoCollectionsExtract,
          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract);
        export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
          import.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

        if (import.AcrossCo.Index + 1 == Import.AcrossCoGroup.Capacity)
        {
          break;
        }

        ++import.AcrossCo.Index;
        import.AcrossCo.CheckSize();
      }

      if (import.AcrossCo.Item.DownCo.Index + 1 == Import.DownCoGroup.Capacity)
      {
        break;
      }

      ++import.AcrossCo.Item.DownCo.Index;
      import.AcrossCo.Item.DownCo.CheckSize();
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'C')
    {
      if (Equal(import.CollectionsExtract.ObligationCode, "CS") || Equal
        (import.CollectionsExtract.ObligationCode, "SP"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber,
            import.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 0;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 0;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "GIFT") || Equal
        (import.CollectionsExtract.ObligationCode, "VOL"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 2;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 2;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "MC") || Equal
        (import.CollectionsExtract.ObligationCode, "MS"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 7;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 3;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 3;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 7;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }
      }
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'A')
    {
      if (Equal(import.CollectionsExtract.ObligationCode, "AJ") || Equal
        (import.CollectionsExtract.ObligationCode, "CRCH") || Equal
        (import.CollectionsExtract.ObligationCode, "CS") || Equal
        (import.CollectionsExtract.ObligationCode, "SAJ") || Equal
        (import.CollectionsExtract.ObligationCode, "SP") || Equal
        (import.CollectionsExtract.ObligationCode, "WA") || Equal
        (import.CollectionsExtract.ObligationCode, "718B"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 1;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 1;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "MC") || Equal
        (import.CollectionsExtract.ObligationCode, "MJ") || Equal
        (import.CollectionsExtract.ObligationCode, "MS"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Item.DownCo.Index = 8;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 4;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 4;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossCo.Item.DownCo.Index = 8;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "IJ"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossCo.Index = 2;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 2;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossCo.Index = 3;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 3;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 1;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossCo.Index = 4;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 1;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 4;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossCo.Index = 6;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 6;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 5;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossCo.Index = 7;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 5;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 7;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossCo.Index = 9;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 9;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossCo.Index = 0;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 8;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
            export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossCo.Index = 10;
          export.AcrossCo.CheckSize();

          export.AcrossCo.Item.DownCo.Index = 5;
          export.AcrossCo.Item.DownCo.CheckSize();

          if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
            CaseNumber) && export
            .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossCo.Index = 0;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 8;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;

            export.AcrossCo.Index = 10;
            export.AcrossCo.CheckSize();

            export.AcrossCo.Item.DownCo.Index = 5;
            export.AcrossCo.Item.DownCo.CheckSize();

            export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
              export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
            export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }
        }
      }
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'F')
    {
      // ***
      // *** FEE's are accumulated in the TOTAL column only.
      // *** When a FEE is added to the system, the associated
      // *** Collection has a PROGRAM_APPLIED_TO of spaces
      // ***
      export.AcrossCo.Index = 0;
      export.AcrossCo.CheckSize();

      export.AcrossCo.Item.DownCo.Index = 6;
      export.AcrossCo.Item.DownCo.CheckSize();

      export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.Amount1 =
        export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 + import
        .CollectionsExtract.Amount1;

      if (IsEmpty(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
        CaseNumber) && export
        .AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1 == 0 || !
        Equal(export.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.
          CaseNumber, import.CollectionsExtract.CaseNumber))
      {
        export.AcrossCo.Update.DownCo.Update.DtlCoCommon.Count =
          export.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count + 1;
        export.AcrossCo.Update.DownCo.Update.DtlCoCollectionsExtract.
          CaseNumber = import.CollectionsExtract.CaseNumber;
      }
    }
  }

  private static void MoveCollectionsExtract(CollectionsExtract source,
    CollectionsExtract target)
  {
    target.CaseNumber = source.CaseNumber;
    target.Amount1 = source.Amount1;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A AcrossCoGroup group.</summary>
    [Serializable]
    public class AcrossCoGroup
    {
      /// <summary>
      /// Gets a value of DownCo.
      /// </summary>
      [JsonIgnore]
      public Array<DownCoGroup> DownCo =>
        downCo ??= new(DownCoGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownCo for json serialization.
      /// </summary>
      [JsonPropertyName("downCo")]
      [Computed]
      public IList<DownCoGroup> DownCo_Json
      {
        get => downCo;
        set => DownCo.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownCoGroup> downCo;
    }

    /// <summary>A DownCoGroup group.</summary>
    [Serializable]
    public class DownCoGroup
    {
      /// <summary>
      /// A value of DtlCoCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlCoCollectionsExtract")]
      public CollectionsExtract DtlCoCollectionsExtract
      {
        get => dtlCoCollectionsExtract ??= new();
        set => dtlCoCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlCoCommon.
      /// </summary>
      [JsonPropertyName("dtlCoCommon")]
      public Common DtlCoCommon
      {
        get => dtlCoCommon ??= new();
        set => dtlCoCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlCoCollectionsExtract;
      private Common dtlCoCommon;
    }

    /// <summary>
    /// Gets a value of AcrossCo.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossCoGroup> AcrossCo => acrossCo ??= new(
      AcrossCoGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossCo for json serialization.
    /// </summary>
    [JsonPropertyName("acrossCo")]
    [Computed]
    public IList<AcrossCoGroup> AcrossCo_Json
    {
      get => acrossCo;
      set => AcrossCo.Assign(value);
    }

    /// <summary>
    /// A value of CollectionsExtract.
    /// </summary>
    [JsonPropertyName("collectionsExtract")]
    public CollectionsExtract CollectionsExtract
    {
      get => collectionsExtract ??= new();
      set => collectionsExtract = value;
    }

    private Array<AcrossCoGroup> acrossCo;
    private CollectionsExtract collectionsExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AcrossCoGroup group.</summary>
    [Serializable]
    public class AcrossCoGroup
    {
      /// <summary>
      /// Gets a value of DownCo.
      /// </summary>
      [JsonIgnore]
      public Array<DownCoGroup> DownCo =>
        downCo ??= new(DownCoGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownCo for json serialization.
      /// </summary>
      [JsonPropertyName("downCo")]
      [Computed]
      public IList<DownCoGroup> DownCo_Json
      {
        get => downCo;
        set => DownCo.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownCoGroup> downCo;
    }

    /// <summary>A DownCoGroup group.</summary>
    [Serializable]
    public class DownCoGroup
    {
      /// <summary>
      /// A value of DtlCoCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlCoCollectionsExtract")]
      public CollectionsExtract DtlCoCollectionsExtract
      {
        get => dtlCoCollectionsExtract ??= new();
        set => dtlCoCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlCoCommon.
      /// </summary>
      [JsonPropertyName("dtlCoCommon")]
      public Common DtlCoCommon
      {
        get => dtlCoCommon ??= new();
        set => dtlCoCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlCoCollectionsExtract;
      private Common dtlCoCommon;
    }

    /// <summary>
    /// Gets a value of AcrossCo.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossCoGroup> AcrossCo => acrossCo ??= new(
      AcrossCoGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossCo for json serialization.
    /// </summary>
    [JsonPropertyName("acrossCo")]
    [Computed]
    public IList<AcrossCoGroup> AcrossCo_Json
    {
      get => acrossCo;
      set => AcrossCo.Assign(value);
    }

    private Array<AcrossCoGroup> acrossCo;
  }
#endregion
}

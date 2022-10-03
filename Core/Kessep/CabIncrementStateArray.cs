// Program: CAB_INCREMENT_STATE_ARRAY, ID: 372819900, model: 746.
// Short name: SWEFF750
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_INCREMENT_STATE_ARRAY.
/// </summary>
[Serializable]
public partial class CabIncrementStateArray: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_INCREMENT_STATE_ARRAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabIncrementStateArray(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabIncrementStateArray.
  /// </summary>
  public CabIncrementStateArray(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    import.AcrossState.Item.DownState.Index = 0;
    import.AcrossState.Item.DownState.CheckSize();

    while(import.AcrossState.Item.DownState.Index < Import
      .DownStateGroup.Capacity)
    {
      import.AcrossState.Index = 0;
      import.AcrossState.CheckSize();

      while(import.AcrossState.Index < Import.AcrossStateGroup.Capacity)
      {
        export.AcrossState.Index = import.AcrossState.Index;
        export.AcrossState.CheckSize();

        export.AcrossState.Item.DownState.Index =
          import.AcrossState.Item.DownState.Index;
        export.AcrossState.Item.DownState.CheckSize();

        MoveCollectionsExtract(import.AcrossState.Item.DownState.Item.
          DtlStateCollectionsExtract,
          export.AcrossState.Update.DownState.Update.
            DtlStateCollectionsExtract);
        export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
          import.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

        if (import.AcrossState.Index + 1 == Import.AcrossStateGroup.Capacity)
        {
          break;
        }

        ++import.AcrossState.Index;
        import.AcrossState.CheckSize();
      }

      if (import.AcrossState.Item.DownState.Index + 1 == Import
        .DownStateGroup.Capacity)
      {
        break;
      }

      ++import.AcrossState.Item.DownState.Index;
      import.AcrossState.Item.DownState.CheckSize();
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'C')
    {
      if (Equal(import.CollectionsExtract.ObligationCode, "CS") || Equal
        (import.CollectionsExtract.ObligationCode, "SP"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 0;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 0;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "GIFT") || Equal
        (import.CollectionsExtract.ObligationCode, "VOL"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 2;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 2;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "MC") || Equal
        (import.CollectionsExtract.ObligationCode, "MS"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 7;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 3;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 3;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 7;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
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
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 1;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 1;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
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
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Item.DownState.Index = 8;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 4;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 4;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossState.Item.DownState.Index = 8;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "IJ"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount1;

          export.AcrossState.Index = 2;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 2;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount2;

          export.AcrossState.Index = 3;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 3;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 1;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount3;

          export.AcrossState.Index = 4;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 1;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 4;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount4;

          export.AcrossState.Index = 6;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 6;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 5;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount5;

          export.AcrossState.Index = 7;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 5;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 7;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount6;

          export.AcrossState.Index = 9;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 9;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossState.Index = 0;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 8;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
            Amount1 =
              export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 + import.CollectionsExtract.Amount7;

          export.AcrossState.Index = 10;
          export.AcrossState.CheckSize();

          export.AcrossState.Item.DownState.Index = 5;
          export.AcrossState.Item.DownState.CheckSize();

          if (IsEmpty(export.AcrossState.Item.DownState.Item.
            DtlStateCollectionsExtract.CaseNumber) && export
            .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossState.Item.DownState.Item.
              DtlStateCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossState.Index = 0;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 8;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;

            export.AcrossState.Index = 10;
            export.AcrossState.CheckSize();

            export.AcrossState.Item.DownState.Index = 5;
            export.AcrossState.Item.DownState.CheckSize();

            export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
              export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
            export.AcrossState.Update.DownState.Update.
              DtlStateCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
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
      export.AcrossState.Index = 0;
      export.AcrossState.CheckSize();

      export.AcrossState.Item.DownState.Index = 6;
      export.AcrossState.Item.DownState.CheckSize();

      export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
        Amount1 =
          export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1 + import.CollectionsExtract.Amount1;

      if (IsEmpty(export.AcrossState.Item.DownState.Item.
        DtlStateCollectionsExtract.CaseNumber) && export
        .AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.Amount1 == 0
        || !
        Equal(export.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          CaseNumber, import.CollectionsExtract.CaseNumber))
      {
        export.AcrossState.Update.DownState.Update.DtlStateCommon.Count =
          export.AcrossState.Item.DownState.Item.DtlStateCommon.Count + 1;
        export.AcrossState.Update.DownState.Update.DtlStateCollectionsExtract.
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
    /// <summary>A AcrossStateGroup group.</summary>
    [Serializable]
    public class AcrossStateGroup
    {
      /// <summary>
      /// Gets a value of DownState.
      /// </summary>
      [JsonIgnore]
      public Array<DownStateGroup> DownState => downState ??= new(
        DownStateGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownState for json serialization.
      /// </summary>
      [JsonPropertyName("downState")]
      [Computed]
      public IList<DownStateGroup> DownState_Json
      {
        get => downState;
        set => DownState.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownStateGroup> downState;
    }

    /// <summary>A DownStateGroup group.</summary>
    [Serializable]
    public class DownStateGroup
    {
      /// <summary>
      /// A value of DtlStateCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlStateCollectionsExtract")]
      public CollectionsExtract DtlStateCollectionsExtract
      {
        get => dtlStateCollectionsExtract ??= new();
        set => dtlStateCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlStateCommon.
      /// </summary>
      [JsonPropertyName("dtlStateCommon")]
      public Common DtlStateCommon
      {
        get => dtlStateCommon ??= new();
        set => dtlStateCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlStateCollectionsExtract;
      private Common dtlStateCommon;
    }

    /// <summary>
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// Gets a value of AcrossState.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossStateGroup> AcrossState => acrossState ??= new(
      AcrossStateGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossState for json serialization.
    /// </summary>
    [JsonPropertyName("acrossState")]
    [Computed]
    public IList<AcrossStateGroup> AcrossState_Json
    {
      get => acrossState;
      set => AcrossState.Assign(value);
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

    private Common firstTimeThru;
    private Array<AcrossStateGroup> acrossState;
    private CollectionsExtract collectionsExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AcrossStateGroup group.</summary>
    [Serializable]
    public class AcrossStateGroup
    {
      /// <summary>
      /// Gets a value of DownState.
      /// </summary>
      [JsonIgnore]
      public Array<DownStateGroup> DownState => downState ??= new(
        DownStateGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownState for json serialization.
      /// </summary>
      [JsonPropertyName("downState")]
      [Computed]
      public IList<DownStateGroup> DownState_Json
      {
        get => downState;
        set => DownState.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownStateGroup> downState;
    }

    /// <summary>A DownStateGroup group.</summary>
    [Serializable]
    public class DownStateGroup
    {
      /// <summary>
      /// A value of DtlStateCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlStateCollectionsExtract")]
      public CollectionsExtract DtlStateCollectionsExtract
      {
        get => dtlStateCollectionsExtract ??= new();
        set => dtlStateCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlStateCommon.
      /// </summary>
      [JsonPropertyName("dtlStateCommon")]
      public Common DtlStateCommon
      {
        get => dtlStateCommon ??= new();
        set => dtlStateCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlStateCollectionsExtract;
      private Common dtlStateCommon;
    }

    /// <summary>
    /// Gets a value of AcrossState.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossStateGroup> AcrossState => acrossState ??= new(
      AcrossStateGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossState for json serialization.
    /// </summary>
    [JsonPropertyName("acrossState")]
    [Computed]
    public IList<AcrossStateGroup> AcrossState_Json
    {
      get => acrossState;
      set => AcrossState.Assign(value);
    }

    private Array<AcrossStateGroup> acrossState;
  }
#endregion
}

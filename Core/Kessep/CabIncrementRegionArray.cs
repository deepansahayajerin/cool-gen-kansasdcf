// Program: CAB_INCREMENT_REGION_ARRAY, ID: 372819899, model: 746.
// Short name: SWEFE750
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_INCREMENT_REGION_ARRAY.
/// </summary>
[Serializable]
public partial class CabIncrementRegionArray: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_INCREMENT_REGION_ARRAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabIncrementRegionArray(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabIncrementRegionArray.
  /// </summary>
  public CabIncrementRegionArray(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    import.AcrossRegion.Item.DownRegion.Index = 0;
    import.AcrossRegion.Item.DownRegion.CheckSize();

    while(import.AcrossRegion.Item.DownRegion.Index < Import
      .DownRegionGroup.Capacity)
    {
      import.AcrossRegion.Index = 0;
      import.AcrossRegion.CheckSize();

      while(import.AcrossRegion.Index < Import.AcrossRegionGroup.Capacity)
      {
        export.AcrossRegion.Index = import.AcrossRegion.Index;
        export.AcrossRegion.CheckSize();

        export.AcrossRegion.Item.DownRegion.Index =
          import.AcrossRegion.Item.DownRegion.Index;
        export.AcrossRegion.Item.DownRegion.CheckSize();

        MoveCollectionsExtract(import.AcrossRegion.Item.DownRegion.Item.
          DtlRegionCollectionsExtract,
          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract);
        export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
          import.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

        if (import.AcrossRegion.Index + 1 == Import.AcrossRegionGroup.Capacity)
        {
          break;
        }

        ++import.AcrossRegion.Index;
        import.AcrossRegion.CheckSize();
      }

      if (import.AcrossRegion.Item.DownRegion.Index + 1 == Import
        .DownRegionGroup.Capacity)
      {
        break;
      }

      ++import.AcrossRegion.Item.DownRegion.Index;
      import.AcrossRegion.Item.DownRegion.CheckSize();
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'C')
    {
      if (Equal(import.CollectionsExtract.ObligationCode, "CS") || Equal
        (import.CollectionsExtract.ObligationCode, "SP"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 0;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 0;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "GIFT") || Equal
        (import.CollectionsExtract.ObligationCode, "VOL"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 2;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 2;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "MC") || Equal
        (import.CollectionsExtract.ObligationCode, "MS"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 7;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 3;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 3;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 7;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
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
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 1;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 1;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
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
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Item.DownRegion.Index = 8;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 4;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 4;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;

            export.AcrossRegion.Item.DownRegion.Index = 8;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "IJ"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossRegion.Index = 2;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 2;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossRegion.Index = 3;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 3;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 1;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossRegion.Index = 4;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 1;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 4;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossRegion.Index = 6;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 6;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 5;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossRegion.Index = 7;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 5;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 7;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossRegion.Index = 9;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 9;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
                import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossRegion.Index = 0;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 8;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          export.AcrossRegion.Update.DownRegion.Update.
            DtlRegionCollectionsExtract.Amount1 =
              export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossRegion.Index = 10;
          export.AcrossRegion.CheckSize();

          export.AcrossRegion.Item.DownRegion.Index = 5;
          export.AcrossRegion.Item.DownRegion.CheckSize();

          if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
            DtlRegionCollectionsExtract.CaseNumber) && export
            .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1 == 0 || !
            Equal(export.AcrossRegion.Item.DownRegion.Item.
              DtlRegionCollectionsExtract.CaseNumber,
            import.CollectionsExtract.CaseNumber))
          {
            export.AcrossRegion.Index = 0;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 8;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;

            export.AcrossRegion.Index = 10;
            export.AcrossRegion.CheckSize();

            export.AcrossRegion.Item.DownRegion.Index = 5;
            export.AcrossRegion.Item.DownRegion.CheckSize();

            export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
              export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1
              ;
            export.AcrossRegion.Update.DownRegion.Update.
              DtlRegionCollectionsExtract.CaseNumber =
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
      export.AcrossRegion.Index = 0;
      export.AcrossRegion.CheckSize();

      export.AcrossRegion.Item.DownRegion.Index = 6;
      export.AcrossRegion.Item.DownRegion.CheckSize();

      export.AcrossRegion.Update.DownRegion.Update.DtlRegionCollectionsExtract.
        Amount1 =
          export.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1 + import.CollectionsExtract.Amount1;

      if (IsEmpty(export.AcrossRegion.Item.DownRegion.Item.
        DtlRegionCollectionsExtract.CaseNumber) && export
        .AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1 == 0 || !
        Equal(export.AcrossRegion.Item.DownRegion.Item.
          DtlRegionCollectionsExtract.CaseNumber,
        import.CollectionsExtract.CaseNumber))
      {
        export.AcrossRegion.Update.DownRegion.Update.DtlRegionCommon.Count =
          export.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count + 1;
        export.AcrossRegion.Update.DownRegion.Update.
          DtlRegionCollectionsExtract.CaseNumber =
            import.CollectionsExtract.CaseNumber;
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
    /// <summary>A AcrossRegionGroup group.</summary>
    [Serializable]
    public class AcrossRegionGroup
    {
      /// <summary>
      /// Gets a value of DownRegion.
      /// </summary>
      [JsonIgnore]
      public Array<DownRegionGroup> DownRegion => downRegion ??= new(
        DownRegionGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownRegion for json serialization.
      /// </summary>
      [JsonPropertyName("downRegion")]
      [Computed]
      public IList<DownRegionGroup> DownRegion_Json
      {
        get => downRegion;
        set => DownRegion.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownRegionGroup> downRegion;
    }

    /// <summary>A DownRegionGroup group.</summary>
    [Serializable]
    public class DownRegionGroup
    {
      /// <summary>
      /// A value of DtlRegionCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlRegionCollectionsExtract")]
      public CollectionsExtract DtlRegionCollectionsExtract
      {
        get => dtlRegionCollectionsExtract ??= new();
        set => dtlRegionCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlRegionCommon.
      /// </summary>
      [JsonPropertyName("dtlRegionCommon")]
      public Common DtlRegionCommon
      {
        get => dtlRegionCommon ??= new();
        set => dtlRegionCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlRegionCollectionsExtract;
      private Common dtlRegionCommon;
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
    /// Gets a value of AcrossRegion.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossRegionGroup> AcrossRegion => acrossRegion ??= new(
      AcrossRegionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossRegion for json serialization.
    /// </summary>
    [JsonPropertyName("acrossRegion")]
    [Computed]
    public IList<AcrossRegionGroup> AcrossRegion_Json
    {
      get => acrossRegion;
      set => AcrossRegion.Assign(value);
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
    private Array<AcrossRegionGroup> acrossRegion;
    private CollectionsExtract collectionsExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AcrossRegionGroup group.</summary>
    [Serializable]
    public class AcrossRegionGroup
    {
      /// <summary>
      /// Gets a value of DownRegion.
      /// </summary>
      [JsonIgnore]
      public Array<DownRegionGroup> DownRegion => downRegion ??= new(
        DownRegionGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownRegion for json serialization.
      /// </summary>
      [JsonPropertyName("downRegion")]
      [Computed]
      public IList<DownRegionGroup> DownRegion_Json
      {
        get => downRegion;
        set => DownRegion.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownRegionGroup> downRegion;
    }

    /// <summary>A DownRegionGroup group.</summary>
    [Serializable]
    public class DownRegionGroup
    {
      /// <summary>
      /// A value of DtlRegionCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlRegionCollectionsExtract")]
      public CollectionsExtract DtlRegionCollectionsExtract
      {
        get => dtlRegionCollectionsExtract ??= new();
        set => dtlRegionCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlRegionCommon.
      /// </summary>
      [JsonPropertyName("dtlRegionCommon")]
      public Common DtlRegionCommon
      {
        get => dtlRegionCommon ??= new();
        set => dtlRegionCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlRegionCollectionsExtract;
      private Common dtlRegionCommon;
    }

    /// <summary>
    /// Gets a value of AcrossRegion.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossRegionGroup> AcrossRegion => acrossRegion ??= new(
      AcrossRegionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossRegion for json serialization.
    /// </summary>
    [JsonPropertyName("acrossRegion")]
    [Computed]
    public IList<AcrossRegionGroup> AcrossRegion_Json
    {
      get => acrossRegion;
      set => AcrossRegion.Assign(value);
    }

    private Array<AcrossRegionGroup> acrossRegion;
  }
#endregion
}

// Program: CAB_INCREMENT_SS_ARRAY, ID: 372819898, model: 746.
// Short name: SWEFD750
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_INCREMENT_SS_ARRAY.
/// </summary>
[Serializable]
public partial class CabIncrementSsArray: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_INCREMENT_SS_ARRAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabIncrementSsArray(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabIncrementSsArray.
  /// </summary>
  public CabIncrementSsArray(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    import.AcrossSs.Item.DownSs.Index = 0;
    import.AcrossSs.Item.DownSs.CheckSize();

    while(import.AcrossSs.Item.DownSs.Index < Import.DownSsGroup.Capacity)
    {
      import.AcrossSs.Index = 0;
      import.AcrossSs.CheckSize();

      while(import.AcrossSs.Index < Import.AcrossSsGroup.Capacity)
      {
        export.AcrossSs.Index = import.AcrossSs.Index;
        export.AcrossSs.CheckSize();

        export.AcrossSs.Item.DownSs.Index = import.AcrossSs.Item.DownSs.Index;
        export.AcrossSs.Item.DownSs.CheckSize();

        MoveCollectionsExtract(import.AcrossSs.Item.DownSs.Item.
          DtlSsCollectionsExtract,
          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract);
        export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
          import.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

        if (import.AcrossSs.Index + 1 == Import.AcrossSsGroup.Capacity)
        {
          break;
        }

        ++import.AcrossSs.Index;
        import.AcrossSs.CheckSize();
      }

      if (import.AcrossSs.Item.DownSs.Index + 1 == Import.DownSsGroup.Capacity)
      {
        break;
      }

      ++import.AcrossSs.Item.DownSs.Index;
      import.AcrossSs.Item.DownSs.CheckSize();
    }

    if (AsChar(import.CollectionsExtract.AppliedTo) == 'C')
    {
      if (Equal(import.CollectionsExtract.ObligationCode, "CS") || Equal
        (import.CollectionsExtract.ObligationCode, "SP"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 0;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 0;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "GIFT") || Equal
        (import.CollectionsExtract.ObligationCode, "VOL"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 2;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 2;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "MC") || Equal
        (import.CollectionsExtract.ObligationCode, "MS"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 7;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 3;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 3;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 7;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
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
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 1;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 1;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
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
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Item.DownSs.Index = 8;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 4;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 4;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;

            export.AcrossSs.Item.DownSs.Index = 8;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
          }

          return;
        }
      }

      if (Equal(import.CollectionsExtract.ObligationCode, "IJ"))
      {
        if (import.CollectionsExtract.Amount1 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount1;

          export.AcrossSs.Index = 2;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 2;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount2 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount2;

          export.AcrossSs.Index = 3;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 3;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount3 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 1;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount3;

          export.AcrossSs.Index = 4;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 1;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 4;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount4 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount4;

          export.AcrossSs.Index = 6;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 6;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount5 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 5;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount5;

          export.AcrossSs.Index = 7;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 5;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 7;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount6 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount6;

          export.AcrossSs.Index = 9;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 9;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
              CaseNumber = import.CollectionsExtract.CaseNumber;
          }

          return;
        }

        if (import.CollectionsExtract.Amount7 > 0)
        {
          export.AcrossSs.Index = 0;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 8;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
            export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
            .CollectionsExtract.Amount7;

          export.AcrossSs.Index = 10;
          export.AcrossSs.CheckSize();

          export.AcrossSs.Item.DownSs.Index = 5;
          export.AcrossSs.Item.DownSs.CheckSize();

          if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
            CaseNumber) && export
            .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
            Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
              CaseNumber, import.CollectionsExtract.CaseNumber))
          {
            export.AcrossSs.Index = 0;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 8;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;

            export.AcrossSs.Index = 10;
            export.AcrossSs.CheckSize();

            export.AcrossSs.Item.DownSs.Index = 5;
            export.AcrossSs.Item.DownSs.CheckSize();

            export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
              export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
            export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
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
      export.AcrossSs.Index = 0;
      export.AcrossSs.CheckSize();

      export.AcrossSs.Item.DownSs.Index = 6;
      export.AcrossSs.Item.DownSs.CheckSize();

      export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.Amount1 =
        export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 + import
        .CollectionsExtract.Amount1;

      if (IsEmpty(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
        CaseNumber) && export
        .AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1 == 0 || !
        Equal(export.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.
          CaseNumber, import.CollectionsExtract.CaseNumber))
      {
        export.AcrossSs.Update.DownSs.Update.DtlSsCommon.Count =
          export.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count + 1;
        export.AcrossSs.Update.DownSs.Update.DtlSsCollectionsExtract.
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
    /// <summary>A AcrossSsGroup group.</summary>
    [Serializable]
    public class AcrossSsGroup
    {
      /// <summary>
      /// Gets a value of DownSs.
      /// </summary>
      [JsonIgnore]
      public Array<DownSsGroup> DownSs =>
        downSs ??= new(DownSsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownSs for json serialization.
      /// </summary>
      [JsonPropertyName("downSs")]
      [Computed]
      public IList<DownSsGroup> DownSs_Json
      {
        get => downSs;
        set => DownSs.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownSsGroup> downSs;
    }

    /// <summary>A DownSsGroup group.</summary>
    [Serializable]
    public class DownSsGroup
    {
      /// <summary>
      /// A value of DtlSsCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlSsCollectionsExtract")]
      public CollectionsExtract DtlSsCollectionsExtract
      {
        get => dtlSsCollectionsExtract ??= new();
        set => dtlSsCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlSsCommon.
      /// </summary>
      [JsonPropertyName("dtlSsCommon")]
      public Common DtlSsCommon
      {
        get => dtlSsCommon ??= new();
        set => dtlSsCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlSsCollectionsExtract;
      private Common dtlSsCommon;
    }

    /// <summary>
    /// Gets a value of AcrossSs.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossSsGroup> AcrossSs => acrossSs ??= new(
      AcrossSsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossSs for json serialization.
    /// </summary>
    [JsonPropertyName("acrossSs")]
    [Computed]
    public IList<AcrossSsGroup> AcrossSs_Json
    {
      get => acrossSs;
      set => AcrossSs.Assign(value);
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

    private Array<AcrossSsGroup> acrossSs;
    private CollectionsExtract collectionsExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AcrossSsGroup group.</summary>
    [Serializable]
    public class AcrossSsGroup
    {
      /// <summary>
      /// Gets a value of DownSs.
      /// </summary>
      [JsonIgnore]
      public Array<DownSsGroup> DownSs =>
        downSs ??= new(DownSsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DownSs for json serialization.
      /// </summary>
      [JsonPropertyName("downSs")]
      [Computed]
      public IList<DownSsGroup> DownSs_Json
      {
        get => downSs;
        set => DownSs.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Array<DownSsGroup> downSs;
    }

    /// <summary>A DownSsGroup group.</summary>
    [Serializable]
    public class DownSsGroup
    {
      /// <summary>
      /// A value of DtlSsCollectionsExtract.
      /// </summary>
      [JsonPropertyName("dtlSsCollectionsExtract")]
      public CollectionsExtract DtlSsCollectionsExtract
      {
        get => dtlSsCollectionsExtract ??= new();
        set => dtlSsCollectionsExtract = value;
      }

      /// <summary>
      /// A value of DtlSsCommon.
      /// </summary>
      [JsonPropertyName("dtlSsCommon")]
      public Common DtlSsCommon
      {
        get => dtlSsCommon ??= new();
        set => dtlSsCommon = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CollectionsExtract dtlSsCollectionsExtract;
      private Common dtlSsCommon;
    }

    /// <summary>
    /// Gets a value of AcrossSs.
    /// </summary>
    [JsonIgnore]
    public Array<AcrossSsGroup> AcrossSs => acrossSs ??= new(
      AcrossSsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AcrossSs for json serialization.
    /// </summary>
    [JsonPropertyName("acrossSs")]
    [Computed]
    public IList<AcrossSsGroup> AcrossSs_Json
    {
      get => acrossSs;
      set => AcrossSs.Assign(value);
    }

    private Array<AcrossSsGroup> acrossSs;
  }
#endregion
}

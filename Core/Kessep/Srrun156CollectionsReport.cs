// Program: SRRUN156_COLLECTIONS_REPORT, ID: 372819747, model: 746.
// Short name: SWEF750B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SRRUN156_COLLECTIONS_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class Srrun156CollectionsReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SRRUN156_COLLECTIONS_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Srrun156CollectionsReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Srrun156CollectionsReport.
  /// </summary>
  public Srrun156CollectionsReport(IContext context, Import import,
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
    // -------------------------------------------------------------------
    //   Date   Developer    PR#    Description
    // -------- --------- --------- -----------
    // 12/13/99  SWSRCHF  H00082616 Bypass printing the Fees line to the
    //                              report.
    // 12/14/99  SWSRCHF  H00082616 Changed the sub-heading 'Grand Total'
    //                              to 'Total'. Added a new sub-heading
    //                              'Grand Total', these totals include
    //                              the INTEREST totals.
    // 01/11/00  SWSRCHF  H00082616 Date parameter set up incorrectly
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Eof.Flag = "N";
    local.Prev.Office = "";
    local.Prev.SectionSupervisor = "";
    local.Prev.CollectionOfficer = "";
    export.ReportLiterals.SubHeading3 = "Cases";
    local.Found.Flag = "N";
    local.FirstTimeThru.Flag = "Y";

    // ***
    // *** get Program Processing Info
    // ***
    if (ReadProgramProcessingInfo())
    {
      // ** Problem report H00082616
      // ** 01/11/00 SWSRCHF
      // **
      // ** start
      if (Month(entities.ProgramProcessingInfo.ProcessDate) == 1)
      {
        local.ProcessYyyymm.Month = 12;
        local.ProcessYyyymm.Year =
          Year(AddYears(entities.ProgramProcessingInfo.ProcessDate, -1));
      }
      else
      {
        local.ProcessYyyymm.Month =
          Month(AddMonths(entities.ProgramProcessingInfo.ProcessDate, -1));
        local.ProcessYyyymm.Year =
          Year(entities.ProgramProcessingInfo.ProcessDate);
      }

      // ** end
      // **
      // ** 01/11/00 SWSRCHF
      // ** Problem report H00082616
      local.Found.Flag = "Y";
    }

    // ***
    // *** Open the Error Report
    // ***
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = entities.ProgramProcessingInfo.ProcessDate;
    local.NeededToOpen.ProgramName = entities.ProgramProcessingInfo.Name;
    UseCabAccessErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    if (AsChar(local.Found.Flag) == 'N')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabAccessErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ***
      // *** Close the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabAccessErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_CLOSING_EXT_FILE";
      }

      return;
    }

    // ***
    // *** Open sorted extract file
    // ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabReadExtractFile1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ***
    // *** Open report
    // ***
    local.ReportParms.Parm1 = "OF";
    local.ReportParms.Parm2 = "";
    UseEabProduceReport1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ***
    // *** initialize the work arrays
    // ***
    UseCabInitializeArrays3();

    while(AsChar(local.Eof.Flag) == 'N')
    {
      // ***
      // *** Read sorted extract file
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      UseEabReadExtractFile2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        if (Equal(local.ReportParms.Parm1, "EF"))
        {
          local.Eof.Flag = "Y";

          continue;
        }

        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

      // *** Problem report H00082616
      // *** 12/14/99 SWSRCHF
      // *** start
      local.Interest.Flag = "N";
      local.GrandTotal.Flag = "N";

      // *** end
      // *** 12/14/99 SWSRCHF
      // *** Problem report H00082616
      // *** change of Region
      if (!Equal(local.CollectionsExtract.Office, local.Prev.Office) && AsChar
        (local.FirstTimeThru.Flag) == 'N')
      {
        local.AcrossCo.Item.DownCo.Index = 0;
        local.AcrossCo.Item.DownCo.CheckSize();

        export.CollectionsExtract.CollectionOfficer =
          local.Prev.CollectionOfficer;

        while(local.AcrossCo.Item.DownCo.Index < Local.DownCoGroup.Capacity)
        {
          local.AcrossCo.Index = 0;
          local.AcrossCo.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.XtafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.PaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.GaFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.MhddCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossCo.Item.DownCo.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossCo.Item.DownCo.Index;
                local.AcrossCo.Item.DownCo.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossCo.Item.DownCo.Index;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write a page for the Collection Officer to the report
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "MAIN";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossCo.Item.DownCo.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossCo.Item.DownCo.Index = 7;
            local.AcrossCo.Item.DownCo.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossCo.Item.DownCo.Index + 1 == Local
            .DownCoGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossCo.Item.DownCo.Index = 5;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossCo.Item.DownCo.Index;
          local.AcrossCo.Item.DownCo.CheckSize();
        }

        local.AcrossSs.Item.DownSs.Index = 0;
        local.AcrossSs.Item.DownSs.CheckSize();

        export.CollectionsExtract.CollectionOfficer =
          local.Prev.SectionSupervisor;

        while(local.AcrossSs.Item.DownSs.Index < Local.DownSsGroup.Capacity)
        {
          local.AcrossSs.Index = 0;
          local.AcrossSs.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.XtafCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafFcCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.NaCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.PaCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.GaFcCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.MhddCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossSs.Item.DownSs.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossSs.Item.DownSs.Index;
                local.AcrossSs.Item.DownSs.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossSs.Item.DownSs.Index;
              local.AcrossSs.Item.DownSs.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write a page for the Section Supervisor to the report
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "SS";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossSs.Item.DownSs.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossSs.Item.DownSs.Index = 7;
            local.AcrossSs.Item.DownSs.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossSs.Item.DownSs.Index + 1 == Local
            .DownSsGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossSs.Item.DownSs.Index = 5;
              local.AcrossSs.Item.DownSs.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossSs.Item.DownSs.Index;
          local.AcrossSs.Item.DownSs.CheckSize();
        }

        local.AcrossRegion.Item.DownRegion.Index = 0;
        local.AcrossRegion.Item.DownRegion.CheckSize();

        export.CollectionsExtract.CollectionOfficer = local.Prev.Office;

        while(local.AcrossRegion.Item.DownRegion.Index < Local
          .DownRegionGroup.Capacity)
        {
          local.AcrossRegion.Index = 0;
          local.AcrossRegion.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.TotalCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.TafTotalCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.TafCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.XtafCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.TafFcCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.NaCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.PaCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.GaFcCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          ++local.AcrossRegion.Index;
          local.AcrossRegion.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
              Amount1;
          export.MhddCommon.Count =
            local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossRegion.Item.DownRegion.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossRegion.Item.DownRegion.Index;
                local.AcrossRegion.Item.DownRegion.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossRegion.Item.DownRegion.Index;
              local.AcrossRegion.Item.DownRegion.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write a page for the Region to the report
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "RG";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossRegion.Item.DownRegion.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossRegion.Item.DownRegion.Index = 7;
            local.AcrossRegion.Item.DownRegion.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossRegion.Item.DownRegion.Index + 1 == Local
            .DownRegionGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossRegion.Item.DownRegion.Index = 5;
              local.AcrossRegion.Item.DownRegion.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossRegion.Item.DownRegion.Index;
          local.AcrossRegion.Item.DownRegion.CheckSize();
        }

        // *** initialize Collection Officer, Section Supervisor and
        // *** Region arrays
        UseCabInitializeArrays3();
        local.Prev.Office = local.CollectionsExtract.Office;
        local.Prev.SectionSupervisor =
          local.CollectionsExtract.SectionSupervisor;
        local.Prev.CollectionOfficer =
          local.CollectionsExtract.CollectionOfficer;
      }

      // *** change of Section Supervisor
      if (!Equal(local.CollectionsExtract.SectionSupervisor,
        local.Prev.SectionSupervisor) && AsChar(local.FirstTimeThru.Flag) == 'N'
        )
      {
        local.AcrossCo.Item.DownCo.Index = 0;
        local.AcrossCo.Item.DownCo.CheckSize();

        export.CollectionsExtract.CollectionOfficer =
          local.Prev.CollectionOfficer;

        while(local.AcrossCo.Item.DownCo.Index < Local.DownCoGroup.Capacity)
        {
          local.AcrossCo.Index = 0;
          local.AcrossCo.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.XtafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.PaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.GaFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.MhddCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossCo.Item.DownCo.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossCo.Item.DownCo.Index;
                local.AcrossCo.Item.DownCo.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossCo.Item.DownCo.Index;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write a page for the Collection Officer to the report
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "MAIN";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossCo.Item.DownCo.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossCo.Item.DownCo.Index = 7;
            local.AcrossCo.Item.DownCo.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossCo.Item.DownCo.Index + 1 == Local
            .DownCoGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossCo.Item.DownCo.Index = 5;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossCo.Item.DownCo.Index;
          local.AcrossCo.Item.DownCo.CheckSize();
        }

        local.AcrossSs.Item.DownSs.Index = 0;
        local.AcrossSs.Item.DownSs.CheckSize();

        export.CollectionsExtract.CollectionOfficer =
          local.Prev.SectionSupervisor;

        while(local.AcrossSs.Item.DownSs.Index < Local.DownSsGroup.Capacity)
        {
          local.AcrossSs.Index = 0;
          local.AcrossSs.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.XtafCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.TafFcCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.NaCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.PaCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.GaFcCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          ++local.AcrossSs.Index;
          local.AcrossSs.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
          export.MhddCommon.Count =
            local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossSs.Item.DownSs.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossSs.Item.DownSs.Index;
                local.AcrossSs.Item.DownSs.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              ++local.AcrossCo.Item.DownCo.Index;
              local.AcrossCo.Item.DownCo.CheckSize();

              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossSs.Item.DownSs.Index;
              local.AcrossSs.Item.DownSs.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write a page for the Section Supervisor to the report
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "SS";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossSs.Item.DownSs.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossSs.Item.DownSs.Index = 7;
            local.AcrossSs.Item.DownSs.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossSs.Item.DownSs.Index + 1 == Local
            .DownSsGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossSs.Item.DownSs.Index = 5;
              local.AcrossSs.Item.DownSs.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossSs.Item.DownSs.Index;
          local.AcrossSs.Item.DownSs.CheckSize();
        }

        // *** initialize Collection Officer and Section Supervisor arrays
        UseCabInitializeArrays2();
        local.Prev.SectionSupervisor =
          local.CollectionsExtract.SectionSupervisor;
        local.Prev.CollectionOfficer =
          local.CollectionsExtract.CollectionOfficer;
      }

      // *** change of Collection Officer
      if (!Equal(local.CollectionsExtract.CollectionOfficer,
        local.Prev.CollectionOfficer) && AsChar(local.FirstTimeThru.Flag) == 'N'
        )
      {
        local.AcrossCo.Item.DownCo.Index = 0;
        local.AcrossCo.Item.DownCo.CheckSize();

        export.CollectionsExtract.CollectionOfficer =
          local.Prev.CollectionOfficer;

        while(local.AcrossCo.Item.DownCo.Index < Local.DownCoGroup.Capacity)
        {
          local.AcrossCo.Index = 0;
          local.AcrossCo.CheckSize();

          export.TotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.XtafCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.XtafCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.TafFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.TafFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NonTafTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NonTafTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.NaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.NaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.PaCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.PaCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.StateOnlyTotalCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.StateOnlyTotalCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.GaFcCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.GaFcCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          ++local.AcrossCo.Index;
          local.AcrossCo.CheckSize();

          export.MhddCollectionsExtract.Amount1 =
            local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
          export.MhddCommon.Count =
            local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

          // *** set report Sub Headings
          switch(local.AcrossCo.Item.DownCo.Index + 1)
          {
            case 1:
              export.ReportLiterals.SubHeading1 =
                "Court/Administrative Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 2:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 3:
              export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 4:
              export.ReportLiterals.SubHeading1 = "Medical";
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            case 5:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              break;
            case 6:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.Interest.Flag) == 'N')
              {
                local.TotalCollectionsExtract.Amount1 =
                  export.TotalCollectionsExtract.Amount1;
                local.TotalCommon.Count = export.TotalCommon.Count;
                local.TafTotalCollectionsExtract.Amount1 =
                  export.TafTotalCollectionsExtract.Amount1;
                local.TafTotalCommon.Count = export.TafTotalCommon.Count;
                local.TafCollectionsExtract.Amount1 =
                  export.TafCollectionsExtract.Amount1;
                local.TafCommon.Count = export.TafCommon.Count;
                local.XtafCollectionsExtract.Amount1 =
                  export.XtafCollectionsExtract.Amount1;
                local.XtafCommon.Count = export.XtafCommon.Count;
                local.TafFcCollectionsExtract.Amount1 =
                  export.TafFcCollectionsExtract.Amount1;
                local.TafFcCommon.Count = export.TafFcCommon.Count;
                local.NonTafTotalCollectionsExtract.Amount1 =
                  export.NonTafTotalCollectionsExtract.Amount1;
                local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
                local.NaCollectionsExtract.Amount1 =
                  export.NaCollectionsExtract.Amount1;
                local.NaCommon.Count = export.NaCommon.Count;
                local.PaCollectionsExtract.Amount1 =
                  export.PaCollectionsExtract.Amount1;
                local.PaCommon.Count = export.PaCommon.Count;
                local.StateOnlyTotalCollectionsExtract.Amount1 =
                  export.StateOnlyTotalCollectionsExtract.Amount1;
                local.StateOnlyTotalCommon.Count =
                  export.StateOnlyTotalCommon.Count;
                local.GaFcCollectionsExtract.Amount1 =
                  export.GaFcCollectionsExtract.Amount1;
                local.GaFcCommon.Count = export.GaFcCommon.Count;
                local.MhddCollectionsExtract.Amount1 =
                  export.MhddCollectionsExtract.Amount1;
                local.MhddCommon.Count = export.MhddCommon.Count;
                local.Interest.Flag = "Y";

                ++local.AcrossCo.Item.DownCo.Index;
                local.AcrossCo.Item.DownCo.CheckSize();

                continue;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading1 = "Judgement Interest";
              export.ReportLiterals.SubHeading2 = "Amount";

              break;
            case 7:
              // *** Problem report H00082616
              // *** 12/13/99 SWSRCHF
              // *** start
              ++local.AcrossCo.Item.DownCo.Index;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;

              // *** end
              // *** 12/13/99 SWSRCHF
              // *** Problem report H00082616
              break;
            case 8:
              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.ReportLiterals.SubHeading1 = "Grand Total";
              }
              else
              {
                export.ReportLiterals.SubHeading1 = "Total";
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              export.ReportLiterals.SubHeading2 = "Current Amount";

              break;
            default:
              export.ReportLiterals.SubHeading1 = "";
              export.ReportLiterals.SubHeading2 = "Arrears Amount";

              // *** Problem report H00082616
              // *** 12/14/99 SWSRCHF
              // *** start
              if (AsChar(local.GrandTotal.Flag) == 'Y')
              {
                export.TotalCollectionsExtract.Amount1 += local.
                  TotalCollectionsExtract.Amount1;
                export.TotalCommon.Count += local.TotalCommon.Count;
                export.TafTotalCollectionsExtract.Amount1 += local.
                  TafTotalCollectionsExtract.Amount1;
                export.TafTotalCommon.Count += local.TafTotalCommon.Count;
                export.TafCollectionsExtract.Amount1 += local.
                  TafCollectionsExtract.Amount1;
                export.TafCommon.Count += local.TafCommon.Count;
                export.XtafCollectionsExtract.Amount1 += local.
                  XtafCollectionsExtract.Amount1;
                export.XtafCommon.Count += local.XtafCommon.Count;
                export.TafFcCollectionsExtract.Amount1 += local.
                  TafFcCollectionsExtract.Amount1;
                export.TafFcCommon.Count += local.TafFcCommon.Count;
                export.NonTafTotalCollectionsExtract.Amount1 += local.
                  NonTafTotalCollectionsExtract.Amount1;
                export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
                export.NaCollectionsExtract.Amount1 += local.
                  NaCollectionsExtract.Amount1;
                export.NaCommon.Count += local.NaCommon.Count;
                export.PaCollectionsExtract.Amount1 += local.
                  PaCollectionsExtract.Amount1;
                export.PaCommon.Count += local.PaCommon.Count;
                export.StateOnlyTotalCollectionsExtract.Amount1 += local.
                  StateOnlyTotalCollectionsExtract.Amount1;
                export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
                  Count;
                export.GaFcCollectionsExtract.Amount1 += local.
                  GaFcCollectionsExtract.Amount1;
                export.GaFcCommon.Count += local.GaFcCommon.Count;
                export.MhddCollectionsExtract.Amount1 += local.
                  MhddCollectionsExtract.Amount1;
                export.MhddCommon.Count += local.MhddCommon.Count;
              }

              // *** end
              // *** 12/14/99 SWSRCHF
              // *** Problem report H00082616
              break;
          }

          // ***
          // *** Write to report,
          // *** one page for the Collection Officer
          // ***
          local.ReportParms.Parm1 = "GR";
          local.ReportParms.Parm2 = "";
          local.ReportParms.SubreportCode = "MAIN";
          UseEabProduceReport2();

          if (!IsEmpty(local.ReportParms.Parm1))
          {
            ExitState = "ERROR_WRITING_TO_REPORT_AB";

            return;
          }

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'Y' && local
            .AcrossCo.Item.DownCo.Index == 5)
          {
            local.Interest.Flag = "N";
            local.GrandTotal.Flag = "Y";

            local.AcrossCo.Item.DownCo.Index = 7;
            local.AcrossCo.Item.DownCo.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          if (local.AcrossCo.Item.DownCo.Index + 1 == Local
            .DownCoGroup.Capacity)
          {
            // *** Problem report H00082616
            // *** 12/14/99 SWSRCHF
            // *** start
            if (AsChar(local.Interest.Flag) == 'Y')
            {
              // *** set subscript to retrieve Judgement Interest data from 
              // array
              local.AcrossCo.Item.DownCo.Index = 5;
              local.AcrossCo.Item.DownCo.CheckSize();

              continue;
            }

            local.GrandTotal.Flag = "N";

            // *** end
            // *** 12/14/99 SWSRCHF
            // *** Problem report H00082616
            break;
          }

          ++local.AcrossCo.Item.DownCo.Index;
          local.AcrossCo.Item.DownCo.CheckSize();
        }

        // *** initialize Collection Officer array
        UseCabInitializeArrays1();
        local.Prev.CollectionOfficer =
          local.CollectionsExtract.CollectionOfficer;
      }

      // *** accumulate in Collection Officer array
      UseCabIncrementCoArray();

      // *** accumulate in Section Supervisor array
      UseCabIncrementSsArray();

      // *** accumulate in Region array
      UseCabIncrementRegionArray();

      // *** accumulate in State array
      UseCabIncrementStateArray();
      local.Prev.Office = local.CollectionsExtract.Office;
      local.Prev.SectionSupervisor = local.CollectionsExtract.SectionSupervisor;
      local.Prev.CollectionOfficer = local.CollectionsExtract.CollectionOfficer;

      if (AsChar(local.FirstTimeThru.Flag) == 'Y')
      {
        local.FirstTimeThru.Flag = "N";
      }
    }

    local.AcrossCo.Item.DownCo.Index = 0;
    local.AcrossCo.Item.DownCo.CheckSize();

    export.CollectionsExtract.CollectionOfficer = local.Prev.CollectionOfficer;

    while(local.AcrossCo.Item.DownCo.Index < Local.DownCoGroup.Capacity)
    {
      local.AcrossCo.Index = 0;
      local.AcrossCo.CheckSize();

      export.TotalCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.TotalCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.TafTotalCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.TafTotalCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.TafCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.TafCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.XtafCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.XtafCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.TafFcCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.TafFcCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.NonTafTotalCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.NonTafTotalCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.NaCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.NaCommon.Count = local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.PaCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.PaCommon.Count = local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.StateOnlyTotalCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.StateOnlyTotalCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.GaFcCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.GaFcCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      ++local.AcrossCo.Index;
      local.AcrossCo.CheckSize();

      export.MhddCollectionsExtract.Amount1 =
        local.AcrossCo.Item.DownCo.Item.DtlCoCollectionsExtract.Amount1;
      export.MhddCommon.Count =
        local.AcrossCo.Item.DownCo.Item.DtlCoCommon.Count;

      // *** set report Sub Headings
      switch(local.AcrossCo.Item.DownCo.Index + 1)
      {
        case 1:
          export.ReportLiterals.SubHeading1 = "Court/Administrative Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 2:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 3:
          export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 4:
          export.ReportLiterals.SubHeading1 = "Medical";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 5:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 6:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'N')
          {
            local.TotalCollectionsExtract.Amount1 =
              export.TotalCollectionsExtract.Amount1;
            local.TotalCommon.Count = export.TotalCommon.Count;
            local.TafTotalCollectionsExtract.Amount1 =
              export.TafTotalCollectionsExtract.Amount1;
            local.TafTotalCommon.Count = export.TafTotalCommon.Count;
            local.TafCollectionsExtract.Amount1 =
              export.TafCollectionsExtract.Amount1;
            local.TafCommon.Count = export.TafCommon.Count;
            local.XtafCollectionsExtract.Amount1 =
              export.XtafCollectionsExtract.Amount1;
            local.XtafCommon.Count = export.XtafCommon.Count;
            local.TafFcCollectionsExtract.Amount1 =
              export.TafFcCollectionsExtract.Amount1;
            local.TafFcCommon.Count = export.TafFcCommon.Count;
            local.NonTafTotalCollectionsExtract.Amount1 =
              export.NonTafTotalCollectionsExtract.Amount1;
            local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
            local.NaCollectionsExtract.Amount1 =
              export.NaCollectionsExtract.Amount1;
            local.NaCommon.Count = export.NaCommon.Count;
            local.PaCollectionsExtract.Amount1 =
              export.PaCollectionsExtract.Amount1;
            local.PaCommon.Count = export.PaCommon.Count;
            local.StateOnlyTotalCollectionsExtract.Amount1 =
              export.StateOnlyTotalCollectionsExtract.Amount1;
            local.StateOnlyTotalCommon.Count =
              export.StateOnlyTotalCommon.Count;
            local.GaFcCollectionsExtract.Amount1 =
              export.GaFcCollectionsExtract.Amount1;
            local.GaFcCommon.Count = export.GaFcCommon.Count;
            local.MhddCollectionsExtract.Amount1 =
              export.MhddCollectionsExtract.Amount1;
            local.MhddCommon.Count = export.MhddCommon.Count;
            local.Interest.Flag = "Y";

            ++local.AcrossCo.Item.DownCo.Index;
            local.AcrossCo.Item.DownCo.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading1 = "Judgement Interest";
          export.ReportLiterals.SubHeading2 = "Amount";

          break;
        case 7:
          // *** Problem report H00082616
          // *** 12/13/99 SWSRCHF
          // *** start
          ++local.AcrossCo.Item.DownCo.Index;
          local.AcrossCo.Item.DownCo.CheckSize();

          continue;

          // *** end
          // *** 12/13/99 SWSRCHF
          // *** Problem report H00082616
          break;
        case 8:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.ReportLiterals.SubHeading1 = "Grand Total";
          }
          else
          {
            export.ReportLiterals.SubHeading1 = "Total";
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        default:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.TotalCollectionsExtract.Amount1 += local.
              TotalCollectionsExtract.Amount1;
            export.TotalCommon.Count += local.TotalCommon.Count;
            export.TafTotalCollectionsExtract.Amount1 += local.
              TafTotalCollectionsExtract.Amount1;
            export.TafTotalCommon.Count += local.TafTotalCommon.Count;
            export.TafCollectionsExtract.Amount1 += local.TafCollectionsExtract.
              Amount1;
            export.TafCommon.Count += local.TafCommon.Count;
            export.XtafCollectionsExtract.Amount1 += local.
              XtafCollectionsExtract.Amount1;
            export.XtafCommon.Count += local.XtafCommon.Count;
            export.TafFcCollectionsExtract.Amount1 += local.
              TafFcCollectionsExtract.Amount1;
            export.TafFcCommon.Count += local.TafFcCommon.Count;
            export.NonTafTotalCollectionsExtract.Amount1 += local.
              NonTafTotalCollectionsExtract.Amount1;
            export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
            export.NaCollectionsExtract.Amount1 += local.NaCollectionsExtract.
              Amount1;
            export.NaCommon.Count += local.NaCommon.Count;
            export.PaCollectionsExtract.Amount1 += local.PaCollectionsExtract.
              Amount1;
            export.PaCommon.Count += local.PaCommon.Count;
            export.StateOnlyTotalCollectionsExtract.Amount1 += local.
              StateOnlyTotalCollectionsExtract.Amount1;
            export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
              Count;
            export.GaFcCollectionsExtract.Amount1 += local.
              GaFcCollectionsExtract.Amount1;
            export.GaFcCommon.Count += local.GaFcCommon.Count;
            export.MhddCollectionsExtract.Amount1 += local.
              MhddCollectionsExtract.Amount1;
            export.MhddCommon.Count += local.MhddCommon.Count;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          break;
      }

      // ***
      // *** Write a page for the Collection Officer to the report
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      local.ReportParms.SubreportCode = "MAIN";
      UseEabProduceReport2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // *** Problem report H00082616
      // *** 12/14/99 SWSRCHF
      // *** start
      if (AsChar(local.Interest.Flag) == 'Y' && local
        .AcrossCo.Item.DownCo.Index == 5)
      {
        local.Interest.Flag = "N";
        local.GrandTotal.Flag = "Y";

        local.AcrossCo.Item.DownCo.Index = 7;
        local.AcrossCo.Item.DownCo.CheckSize();

        continue;
      }

      // *** end
      // *** 12/14/99 SWSRCHF
      // *** Problem report H00082616
      if (local.AcrossCo.Item.DownCo.Index + 1 == Local.DownCoGroup.Capacity)
      {
        // *** Problem report H00082616
        // *** 12/14/99 SWSRCHF
        // *** start
        if (AsChar(local.Interest.Flag) == 'Y')
        {
          // *** set subscript to retrieve Judgement Interest data from array
          local.AcrossCo.Item.DownCo.Index = 5;
          local.AcrossCo.Item.DownCo.CheckSize();

          continue;
        }

        local.GrandTotal.Flag = "N";

        // *** end
        // *** 12/14/99 SWSRCHF
        // *** Problem report H00082616
        break;
      }

      ++local.AcrossCo.Item.DownCo.Index;
      local.AcrossCo.Item.DownCo.CheckSize();
    }

    local.AcrossSs.Item.DownSs.Index = 0;
    local.AcrossSs.Item.DownSs.CheckSize();

    export.CollectionsExtract.CollectionOfficer = local.Prev.SectionSupervisor;

    while(local.AcrossSs.Item.DownSs.Index < Local.DownSsGroup.Capacity)
    {
      local.AcrossSs.Index = 0;
      local.AcrossSs.CheckSize();

      export.TotalCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.TotalCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.TafTotalCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.TafTotalCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.TafCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.TafCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.XtafCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.XtafCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.TafFcCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.TafFcCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.NonTafTotalCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.NonTafTotalCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.NaCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.NaCommon.Count = local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.PaCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.PaCommon.Count = local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.StateOnlyTotalCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.StateOnlyTotalCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.GaFcCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.GaFcCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      ++local.AcrossSs.Index;
      local.AcrossSs.CheckSize();

      export.MhddCollectionsExtract.Amount1 =
        local.AcrossSs.Item.DownSs.Item.DtlSsCollectionsExtract.Amount1;
      export.MhddCommon.Count =
        local.AcrossSs.Item.DownSs.Item.DtlSsCommon.Count;

      // *** set report Sub Headings
      switch(local.AcrossSs.Item.DownSs.Index + 1)
      {
        case 1:
          export.ReportLiterals.SubHeading1 = "Court/Administrative Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 2:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 3:
          export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 4:
          export.ReportLiterals.SubHeading1 = "Medical";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 5:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 6:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'N')
          {
            local.TotalCollectionsExtract.Amount1 =
              export.TotalCollectionsExtract.Amount1;
            local.TotalCommon.Count = export.TotalCommon.Count;
            local.TafTotalCollectionsExtract.Amount1 =
              export.TafTotalCollectionsExtract.Amount1;
            local.TafTotalCommon.Count = export.TafTotalCommon.Count;
            local.TafCollectionsExtract.Amount1 =
              export.TafCollectionsExtract.Amount1;
            local.TafCommon.Count = export.TafCommon.Count;
            local.XtafCollectionsExtract.Amount1 =
              export.XtafCollectionsExtract.Amount1;
            local.XtafCommon.Count = export.XtafCommon.Count;
            local.TafFcCollectionsExtract.Amount1 =
              export.TafFcCollectionsExtract.Amount1;
            local.TafFcCommon.Count = export.TafFcCommon.Count;
            local.NonTafTotalCollectionsExtract.Amount1 =
              export.NonTafTotalCollectionsExtract.Amount1;
            local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
            local.NaCollectionsExtract.Amount1 =
              export.NaCollectionsExtract.Amount1;
            local.NaCommon.Count = export.NaCommon.Count;
            local.PaCollectionsExtract.Amount1 =
              export.PaCollectionsExtract.Amount1;
            local.PaCommon.Count = export.PaCommon.Count;
            local.StateOnlyTotalCollectionsExtract.Amount1 =
              export.StateOnlyTotalCollectionsExtract.Amount1;
            local.StateOnlyTotalCommon.Count =
              export.StateOnlyTotalCommon.Count;
            local.GaFcCollectionsExtract.Amount1 =
              export.GaFcCollectionsExtract.Amount1;
            local.GaFcCommon.Count = export.GaFcCommon.Count;
            local.MhddCollectionsExtract.Amount1 =
              export.MhddCollectionsExtract.Amount1;
            local.MhddCommon.Count = export.MhddCommon.Count;
            local.Interest.Flag = "Y";

            ++local.AcrossSs.Item.DownSs.Index;
            local.AcrossSs.Item.DownSs.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading1 = "Judgement Interest";
          export.ReportLiterals.SubHeading2 = "Amount";

          break;
        case 7:
          // *** Problem report H00082616
          // *** 12/13/99 SWSRCHF
          // *** start
          ++local.AcrossSs.Item.DownSs.Index;
          local.AcrossSs.Item.DownSs.CheckSize();

          continue;

          // *** end
          // *** 12/13/99 SWSRCHF
          // *** Problem report H00082616
          break;
        case 8:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.ReportLiterals.SubHeading1 = "Grand Total";
          }
          else
          {
            export.ReportLiterals.SubHeading1 = "Total";
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        default:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.TotalCollectionsExtract.Amount1 += local.
              TotalCollectionsExtract.Amount1;
            export.TotalCommon.Count += local.TotalCommon.Count;
            export.TafTotalCollectionsExtract.Amount1 += local.
              TafTotalCollectionsExtract.Amount1;
            export.TafTotalCommon.Count += local.TafTotalCommon.Count;
            export.TafCollectionsExtract.Amount1 += local.TafCollectionsExtract.
              Amount1;
            export.TafCommon.Count += local.TafCommon.Count;
            export.XtafCollectionsExtract.Amount1 += local.
              XtafCollectionsExtract.Amount1;
            export.XtafCommon.Count += local.XtafCommon.Count;
            export.TafFcCollectionsExtract.Amount1 += local.
              TafFcCollectionsExtract.Amount1;
            export.TafFcCommon.Count += local.TafFcCommon.Count;
            export.NonTafTotalCollectionsExtract.Amount1 += local.
              NonTafTotalCollectionsExtract.Amount1;
            export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
            export.NaCollectionsExtract.Amount1 += local.NaCollectionsExtract.
              Amount1;
            export.NaCommon.Count += local.NaCommon.Count;
            export.PaCollectionsExtract.Amount1 += local.PaCollectionsExtract.
              Amount1;
            export.PaCommon.Count += local.PaCommon.Count;
            export.StateOnlyTotalCollectionsExtract.Amount1 += local.
              StateOnlyTotalCollectionsExtract.Amount1;
            export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
              Count;
            export.GaFcCollectionsExtract.Amount1 += local.
              GaFcCollectionsExtract.Amount1;
            export.GaFcCommon.Count += local.GaFcCommon.Count;
            export.MhddCollectionsExtract.Amount1 += local.
              MhddCollectionsExtract.Amount1;
            export.MhddCommon.Count += local.MhddCommon.Count;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          break;
      }

      // ***
      // *** Write a page for the Section Supervisor to the report
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      local.ReportParms.SubreportCode = "SS";
      UseEabProduceReport2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // *** Problem report H00082616
      // *** 12/14/99 SWSRCHF
      // *** start
      if (AsChar(local.Interest.Flag) == 'Y' && local
        .AcrossSs.Item.DownSs.Index == 5)
      {
        local.Interest.Flag = "N";
        local.GrandTotal.Flag = "Y";

        local.AcrossSs.Item.DownSs.Index = 7;
        local.AcrossSs.Item.DownSs.CheckSize();

        continue;
      }

      // *** end
      // *** 12/14/99 SWSRCHF
      // *** Problem report H00082616
      if (local.AcrossSs.Item.DownSs.Index + 1 == Local.DownSsGroup.Capacity)
      {
        // *** Problem report H00082616
        // *** 12/14/99 SWSRCHF
        // *** start
        if (AsChar(local.Interest.Flag) == 'Y')
        {
          // *** set subscript to retrieve Judgement Interest data from array
          local.AcrossSs.Item.DownSs.Index = 5;
          local.AcrossSs.Item.DownSs.CheckSize();

          continue;
        }

        local.GrandTotal.Flag = "N";

        // *** end
        // *** 12/14/99 SWSRCHF
        // *** Problem report H00082616
        break;
      }

      ++local.AcrossSs.Item.DownSs.Index;
      local.AcrossSs.Item.DownSs.CheckSize();
    }

    local.AcrossRegion.Item.DownRegion.Index = 0;
    local.AcrossRegion.Item.DownRegion.CheckSize();

    export.CollectionsExtract.CollectionOfficer = local.Prev.Office;

    while(local.AcrossRegion.Item.DownRegion.Index < Local
      .DownRegionGroup.Capacity)
    {
      local.AcrossRegion.Index = 0;
      local.AcrossRegion.CheckSize();

      export.TotalCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.TotalCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.TafTotalCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.TafTotalCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.TafCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.TafCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.XtafCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.XtafCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.TafFcCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.TafFcCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.NonTafTotalCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.NonTafTotalCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.NaCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.NaCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.PaCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.PaCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.StateOnlyTotalCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.StateOnlyTotalCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.GaFcCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.GaFcCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      ++local.AcrossRegion.Index;
      local.AcrossRegion.CheckSize();

      export.MhddCollectionsExtract.Amount1 =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCollectionsExtract.
          Amount1;
      export.MhddCommon.Count =
        local.AcrossRegion.Item.DownRegion.Item.DtlRegionCommon.Count;

      // *** set report Sub Headings
      switch(local.AcrossRegion.Item.DownRegion.Index + 1)
      {
        case 1:
          export.ReportLiterals.SubHeading1 = "Court/Administrative Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 2:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 3:
          export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 4:
          export.ReportLiterals.SubHeading1 = "Medical";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 5:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 6:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'N')
          {
            local.TotalCollectionsExtract.Amount1 =
              export.TotalCollectionsExtract.Amount1;
            local.TotalCommon.Count = export.TotalCommon.Count;
            local.TafTotalCollectionsExtract.Amount1 =
              export.TafTotalCollectionsExtract.Amount1;
            local.TafTotalCommon.Count = export.TafTotalCommon.Count;
            local.TafCollectionsExtract.Amount1 =
              export.TafCollectionsExtract.Amount1;
            local.TafCommon.Count = export.TafCommon.Count;
            local.XtafCollectionsExtract.Amount1 =
              export.XtafCollectionsExtract.Amount1;
            local.XtafCommon.Count = export.XtafCommon.Count;
            local.TafFcCollectionsExtract.Amount1 =
              export.TafFcCollectionsExtract.Amount1;
            local.TafFcCommon.Count = export.TafFcCommon.Count;
            local.NonTafTotalCollectionsExtract.Amount1 =
              export.NonTafTotalCollectionsExtract.Amount1;
            local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
            local.NaCollectionsExtract.Amount1 =
              export.NaCollectionsExtract.Amount1;
            local.NaCommon.Count = export.NaCommon.Count;
            local.PaCollectionsExtract.Amount1 =
              export.PaCollectionsExtract.Amount1;
            local.PaCommon.Count = export.PaCommon.Count;
            local.StateOnlyTotalCollectionsExtract.Amount1 =
              export.StateOnlyTotalCollectionsExtract.Amount1;
            local.StateOnlyTotalCommon.Count =
              export.StateOnlyTotalCommon.Count;
            local.GaFcCollectionsExtract.Amount1 =
              export.GaFcCollectionsExtract.Amount1;
            local.GaFcCommon.Count = export.GaFcCommon.Count;
            local.MhddCollectionsExtract.Amount1 =
              export.MhddCollectionsExtract.Amount1;
            local.MhddCommon.Count = export.MhddCommon.Count;
            local.Interest.Flag = "Y";

            ++local.AcrossRegion.Item.DownRegion.Index;
            local.AcrossRegion.Item.DownRegion.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading1 = "Judgement Interest";
          export.ReportLiterals.SubHeading2 = "Amount";

          break;
        case 7:
          // *** Problem report H00082616
          // *** 12/13/99 SWSRCHF
          // *** start
          ++local.AcrossRegion.Item.DownRegion.Index;
          local.AcrossRegion.Item.DownRegion.CheckSize();

          continue;

          // *** end
          // *** 12/13/99 SWSRCHF
          // *** Problem report H00082616
          break;
        case 8:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.ReportLiterals.SubHeading1 = "Grand Total";
          }
          else
          {
            export.ReportLiterals.SubHeading1 = "Total";
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        default:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.TotalCollectionsExtract.Amount1 += local.
              TotalCollectionsExtract.Amount1;
            export.TotalCommon.Count += local.TotalCommon.Count;
            export.TafTotalCollectionsExtract.Amount1 += local.
              TafTotalCollectionsExtract.Amount1;
            export.TafTotalCommon.Count += local.TafTotalCommon.Count;
            export.TafCollectionsExtract.Amount1 += local.TafCollectionsExtract.
              Amount1;
            export.TafCommon.Count += local.TafCommon.Count;
            export.XtafCollectionsExtract.Amount1 += local.
              XtafCollectionsExtract.Amount1;
            export.XtafCommon.Count += local.XtafCommon.Count;
            export.TafFcCollectionsExtract.Amount1 += local.
              TafFcCollectionsExtract.Amount1;
            export.TafFcCommon.Count += local.TafFcCommon.Count;
            export.NonTafTotalCollectionsExtract.Amount1 += local.
              NonTafTotalCollectionsExtract.Amount1;
            export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
            export.NaCollectionsExtract.Amount1 += local.NaCollectionsExtract.
              Amount1;
            export.NaCommon.Count += local.NaCommon.Count;
            export.PaCollectionsExtract.Amount1 += local.PaCollectionsExtract.
              Amount1;
            export.PaCommon.Count += local.PaCommon.Count;
            export.StateOnlyTotalCollectionsExtract.Amount1 += local.
              StateOnlyTotalCollectionsExtract.Amount1;
            export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
              Count;
            export.GaFcCollectionsExtract.Amount1 += local.
              GaFcCollectionsExtract.Amount1;
            export.GaFcCommon.Count += local.GaFcCommon.Count;
            export.MhddCollectionsExtract.Amount1 += local.
              MhddCollectionsExtract.Amount1;
            export.MhddCommon.Count += local.MhddCommon.Count;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          break;
      }

      // ***
      // *** Write a page for the Region to the report
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      local.ReportParms.SubreportCode = "RG";
      UseEabProduceReport2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // *** Problem report H00082616
      // *** 12/14/99 SWSRCHF
      // *** start
      if (AsChar(local.Interest.Flag) == 'Y' && local
        .AcrossRegion.Item.DownRegion.Index == 5)
      {
        local.Interest.Flag = "N";
        local.GrandTotal.Flag = "Y";

        local.AcrossRegion.Item.DownRegion.Index = 7;
        local.AcrossRegion.Item.DownRegion.CheckSize();

        continue;
      }

      // *** end
      // *** 12/14/99 SWSRCHF
      // *** Problem report H00082616
      if (local.AcrossRegion.Item.DownRegion.Index + 1 == Local
        .DownRegionGroup.Capacity)
      {
        // *** Problem report H00082616
        // *** 12/14/99 SWSRCHF
        // *** start
        if (AsChar(local.Interest.Flag) == 'Y')
        {
          // *** set subscript to retrieve Judgement Interest data from array
          local.AcrossRegion.Item.DownRegion.Index = 5;
          local.AcrossRegion.Item.DownRegion.CheckSize();

          continue;
        }

        local.GrandTotal.Flag = "N";

        // *** end
        // *** 12/14/99 SWSRCHF
        // *** Problem report H00082616
        break;
      }

      ++local.AcrossRegion.Item.DownRegion.Index;
      local.AcrossRegion.Item.DownRegion.CheckSize();
    }

    local.AcrossState.Item.DownState.Index = 0;
    local.AcrossState.Item.DownState.CheckSize();

    export.CollectionsExtract.CollectionOfficer = "State of Kansas";

    while(local.AcrossState.Item.DownState.Index < Local
      .DownStateGroup.Capacity)
    {
      local.AcrossState.Index = 0;
      local.AcrossState.CheckSize();

      export.TotalCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.TotalCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.TafTotalCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.TafTotalCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.TafCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.TafCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.XtafCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.XtafCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.TafFcCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.TafFcCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.NonTafTotalCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.NonTafTotalCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.NaCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.NaCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.PaCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.PaCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.StateOnlyTotalCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.StateOnlyTotalCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.GaFcCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.GaFcCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      ++local.AcrossState.Index;
      local.AcrossState.CheckSize();

      export.MhddCollectionsExtract.Amount1 =
        local.AcrossState.Item.DownState.Item.DtlStateCollectionsExtract.
          Amount1;
      export.MhddCommon.Count =
        local.AcrossState.Item.DownState.Item.DtlStateCommon.Count;

      // *** set report Sub Headings
      switch(local.AcrossState.Item.DownState.Index + 1)
      {
        case 1:
          export.ReportLiterals.SubHeading1 = "Court/Administrative Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 2:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 3:
          export.ReportLiterals.SubHeading1 = "Non Court Ordered:";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 4:
          export.ReportLiterals.SubHeading1 = "Medical";
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        case 5:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          break;
        case 6:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.Interest.Flag) == 'N')
          {
            local.TotalCollectionsExtract.Amount1 =
              export.TotalCollectionsExtract.Amount1;
            local.TotalCommon.Count = export.TotalCommon.Count;
            local.TafTotalCollectionsExtract.Amount1 =
              export.TafTotalCollectionsExtract.Amount1;
            local.TafTotalCommon.Count = export.TafTotalCommon.Count;
            local.TafCollectionsExtract.Amount1 =
              export.TafCollectionsExtract.Amount1;
            local.TafCommon.Count = export.TafCommon.Count;
            local.XtafCollectionsExtract.Amount1 =
              export.XtafCollectionsExtract.Amount1;
            local.XtafCommon.Count = export.XtafCommon.Count;
            local.TafFcCollectionsExtract.Amount1 =
              export.TafFcCollectionsExtract.Amount1;
            local.TafFcCommon.Count = export.TafFcCommon.Count;
            local.NonTafTotalCollectionsExtract.Amount1 =
              export.NonTafTotalCollectionsExtract.Amount1;
            local.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
            local.NaCollectionsExtract.Amount1 =
              export.NaCollectionsExtract.Amount1;
            local.NaCommon.Count = export.NaCommon.Count;
            local.PaCollectionsExtract.Amount1 =
              export.PaCollectionsExtract.Amount1;
            local.PaCommon.Count = export.PaCommon.Count;
            local.StateOnlyTotalCollectionsExtract.Amount1 =
              export.StateOnlyTotalCollectionsExtract.Amount1;
            local.StateOnlyTotalCommon.Count =
              export.StateOnlyTotalCommon.Count;
            local.GaFcCollectionsExtract.Amount1 =
              export.GaFcCollectionsExtract.Amount1;
            local.GaFcCommon.Count = export.GaFcCommon.Count;
            local.MhddCollectionsExtract.Amount1 =
              export.MhddCollectionsExtract.Amount1;
            local.MhddCommon.Count = export.MhddCommon.Count;
            local.Interest.Flag = "Y";

            ++local.AcrossState.Item.DownState.Index;
            local.AcrossState.Item.DownState.CheckSize();

            continue;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading1 = "Judgement Interest";
          export.ReportLiterals.SubHeading2 = "Amount";

          break;
        case 7:
          // *** Problem report H00082616
          // *** 12/13/99 SWSRCHF
          // *** start
          ++local.AcrossState.Item.DownState.Index;
          local.AcrossState.Item.DownState.CheckSize();

          continue;

          // *** end
          // *** 12/13/99 SWSRCHF
          // *** Problem report H00082616
          break;
        case 8:
          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.ReportLiterals.SubHeading1 = "Grand Total";
          }
          else
          {
            export.ReportLiterals.SubHeading1 = "Total";
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          export.ReportLiterals.SubHeading2 = "Current Amount";

          break;
        default:
          export.ReportLiterals.SubHeading1 = "";
          export.ReportLiterals.SubHeading2 = "Arrears Amount";

          // *** Problem report H00082616
          // *** 12/14/99 SWSRCHF
          // *** start
          if (AsChar(local.GrandTotal.Flag) == 'Y')
          {
            export.TotalCollectionsExtract.Amount1 += local.
              TotalCollectionsExtract.Amount1;
            export.TotalCommon.Count += local.TotalCommon.Count;
            export.TafTotalCollectionsExtract.Amount1 += local.
              TafTotalCollectionsExtract.Amount1;
            export.TafTotalCommon.Count += local.TafTotalCommon.Count;
            export.TafCollectionsExtract.Amount1 += local.TafCollectionsExtract.
              Amount1;
            export.TafCommon.Count += local.TafCommon.Count;
            export.XtafCollectionsExtract.Amount1 += local.
              XtafCollectionsExtract.Amount1;
            export.XtafCommon.Count += local.XtafCommon.Count;
            export.TafFcCollectionsExtract.Amount1 += local.
              TafFcCollectionsExtract.Amount1;
            export.TafFcCommon.Count += local.TafFcCommon.Count;
            export.NonTafTotalCollectionsExtract.Amount1 += local.
              NonTafTotalCollectionsExtract.Amount1;
            export.NonTafTotalCommon.Count += local.NonTafTotalCommon.Count;
            export.NaCollectionsExtract.Amount1 += local.NaCollectionsExtract.
              Amount1;
            export.NaCommon.Count += local.NaCommon.Count;
            export.PaCollectionsExtract.Amount1 += local.PaCollectionsExtract.
              Amount1;
            export.PaCommon.Count += local.PaCommon.Count;
            export.StateOnlyTotalCollectionsExtract.Amount1 += local.
              StateOnlyTotalCollectionsExtract.Amount1;
            export.StateOnlyTotalCommon.Count += local.StateOnlyTotalCommon.
              Count;
            export.GaFcCollectionsExtract.Amount1 += local.
              GaFcCollectionsExtract.Amount1;
            export.GaFcCommon.Count += local.GaFcCommon.Count;
            export.MhddCollectionsExtract.Amount1 += local.
              MhddCollectionsExtract.Amount1;
            export.MhddCommon.Count += local.MhddCommon.Count;
          }

          // *** end
          // *** 12/14/99 SWSRCHF
          // *** Problem report H00082616
          break;
      }

      // ***
      // *** Write a page for the State to the report
      // ***
      local.ReportParms.Parm1 = "GR";
      local.ReportParms.Parm2 = "";
      local.ReportParms.SubreportCode = "ST";
      UseEabProduceReport2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // *** Problem report H00082616
      // *** 12/14/99 SWSRCHF
      // *** start
      if (AsChar(local.Interest.Flag) == 'Y' && local
        .AcrossState.Item.DownState.Index == 5)
      {
        local.Interest.Flag = "N";
        local.GrandTotal.Flag = "Y";

        local.AcrossState.Item.DownState.Index = 7;
        local.AcrossState.Item.DownState.CheckSize();

        continue;
      }

      // *** end
      // *** 12/14/99 SWSRCHF
      // *** Problem report H00082616
      if (local.AcrossState.Item.DownState.Index + 1 == Local
        .DownStateGroup.Capacity)
      {
        // *** Problem report H00082616
        // *** 12/14/99 SWSRCHF
        // *** start
        if (AsChar(local.Interest.Flag) == 'Y')
        {
          // *** set subscript to retrieve Judgement Interest data from array
          local.AcrossState.Item.DownState.Index = 5;
          local.AcrossState.Item.DownState.CheckSize();

          continue;
        }

        local.GrandTotal.Flag = "N";

        // *** end
        // *** 12/14/99 SWSRCHF
        // *** Problem report H00082616
        break;
      }

      ++local.AcrossState.Item.DownState.Index;
      local.AcrossState.Item.DownState.CheckSize();
    }

    // ***
    // *** Close sorted extract file
    // ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabReadExtractFile1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // ***
    // *** Close report
    // ***
    local.ReportParms.Parm1 = "CF";
    local.ReportParms.Parm2 = "";
    UseEabProduceReport1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // ***
    // *** Close the Error Report
    // ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabAccessErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";
    }
  }

  private static void MoveAcrossCo1(Local.AcrossCoGroup source,
    CabIncrementCoArray.Import.AcrossCoGroup target)
  {
    source.DownCo.CopyTo(target.DownCo, MoveDownCo1);
  }

  private static void MoveAcrossCo2(CabIncrementCoArray.Export.
    AcrossCoGroup source, Local.AcrossCoGroup target)
  {
    source.DownCo.CopyTo(target.DownCo, MoveDownCo2);
  }

  private static void MoveAcrossCo3(CabInitializeArrays.Export.
    AcrossCoGroup source, Local.AcrossCoGroup target)
  {
    source.DownCo.CopyTo(target.DownCo, MoveDownCo3);
  }

  private static void MoveAcrossRegion1(Local.AcrossRegionGroup source,
    CabIncrementRegionArray.Import.AcrossRegionGroup target)
  {
    source.DownRegion.CopyTo(target.DownRegion, MoveDownRegion1);
  }

  private static void MoveAcrossRegion2(CabIncrementRegionArray.Export.
    AcrossRegionGroup source, Local.AcrossRegionGroup target)
  {
    source.DownRegion.CopyTo(target.DownRegion, MoveDownRegion2);
  }

  private static void MoveAcrossRegion3(CabInitializeArrays.Export.
    AcrossRegionGroup source, Local.AcrossRegionGroup target)
  {
    source.DownRegion.CopyTo(target.DownRegion, MoveDownRegion3);
  }

  private static void MoveAcrossSs1(Local.AcrossSsGroup source,
    CabIncrementSsArray.Import.AcrossSsGroup target)
  {
    source.DownSs.CopyTo(target.DownSs, MoveDownSs1);
  }

  private static void MoveAcrossSs2(CabIncrementSsArray.Export.
    AcrossSsGroup source, Local.AcrossSsGroup target)
  {
    source.DownSs.CopyTo(target.DownSs, MoveDownSs2);
  }

  private static void MoveAcrossSs3(CabInitializeArrays.Export.
    AcrossSsGroup source, Local.AcrossSsGroup target)
  {
    source.DownSs.CopyTo(target.DownSs, MoveDownSs3);
  }

  private static void MoveAcrossState1(Local.AcrossStateGroup source,
    CabIncrementStateArray.Import.AcrossStateGroup target)
  {
    source.DownState.CopyTo(target.DownState, MoveDownState1);
  }

  private static void MoveAcrossState2(CabIncrementStateArray.Export.
    AcrossStateGroup source, Local.AcrossStateGroup target)
  {
    source.DownState.CopyTo(target.DownState, MoveDownState2);
  }

  private static void MoveCollectionsExtract(CollectionsExtract source,
    CollectionsExtract target)
  {
    target.CaseNumber = source.CaseNumber;
    target.Amount1 = source.Amount1;
  }

  private static void MoveDownCo1(Local.DownCoGroup source,
    CabIncrementCoArray.Import.DownCoGroup target)
  {
    MoveCollectionsExtract(source.DtlCoCollectionsExtract,
      target.DtlCoCollectionsExtract);
    target.DtlCoCommon.Count = source.DtlCoCommon.Count;
  }

  private static void MoveDownCo2(CabIncrementCoArray.Export.DownCoGroup source,
    Local.DownCoGroup target)
  {
    MoveCollectionsExtract(source.DtlCoCollectionsExtract,
      target.DtlCoCollectionsExtract);
    target.DtlCoCommon.Count = source.DtlCoCommon.Count;
  }

  private static void MoveDownCo3(CabInitializeArrays.Export.DownCoGroup source,
    Local.DownCoGroup target)
  {
    MoveCollectionsExtract(source.DtlCoCollectionsExtract,
      target.DtlCoCollectionsExtract);
    target.DtlCoCommon.Count = source.DtlCoCommon.Count;
  }

  private static void MoveDownRegion1(Local.DownRegionGroup source,
    CabIncrementRegionArray.Import.DownRegionGroup target)
  {
    MoveCollectionsExtract(source.DtlRegionCollectionsExtract,
      target.DtlRegionCollectionsExtract);
    target.DtlRegionCommon.Count = source.DtlRegionCommon.Count;
  }

  private static void MoveDownRegion2(CabIncrementRegionArray.Export.
    DownRegionGroup source, Local.DownRegionGroup target)
  {
    MoveCollectionsExtract(source.DtlRegionCollectionsExtract,
      target.DtlRegionCollectionsExtract);
    target.DtlRegionCommon.Count = source.DtlRegionCommon.Count;
  }

  private static void MoveDownRegion3(CabInitializeArrays.Export.
    DownRegionGroup source, Local.DownRegionGroup target)
  {
    MoveCollectionsExtract(source.DtlRegionCollectionsExtract,
      target.DtlRegionCollectionsExtract);
    target.DtlRegionCommon.Count = source.DtlRegionCommon.Count;
  }

  private static void MoveDownSs1(Local.DownSsGroup source,
    CabIncrementSsArray.Import.DownSsGroup target)
  {
    MoveCollectionsExtract(source.DtlSsCollectionsExtract,
      target.DtlSsCollectionsExtract);
    target.DtlSsCommon.Count = source.DtlSsCommon.Count;
  }

  private static void MoveDownSs2(CabIncrementSsArray.Export.DownSsGroup source,
    Local.DownSsGroup target)
  {
    MoveCollectionsExtract(source.DtlSsCollectionsExtract,
      target.DtlSsCollectionsExtract);
    target.DtlSsCommon.Count = source.DtlSsCommon.Count;
  }

  private static void MoveDownSs3(CabInitializeArrays.Export.DownSsGroup source,
    Local.DownSsGroup target)
  {
    MoveCollectionsExtract(source.DtlSsCollectionsExtract,
      target.DtlSsCollectionsExtract);
    target.DtlSsCommon.Count = source.DtlSsCommon.Count;
  }

  private static void MoveDownState1(Local.DownStateGroup source,
    CabIncrementStateArray.Import.DownStateGroup target)
  {
    MoveCollectionsExtract(source.DtlStateCollectionsExtract,
      target.DtlStateCollectionsExtract);
    target.DtlStateCommon.Count = source.DtlStateCommon.Count;
  }

  private static void MoveDownState2(CabIncrementStateArray.Export.
    DownStateGroup source, Local.DownStateGroup target)
  {
    MoveCollectionsExtract(source.DtlStateCollectionsExtract,
      target.DtlStateCollectionsExtract);
    target.DtlStateCommon.Count = source.DtlStateCommon.Count;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabAccessErrorReport1()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabAccessErrorReport2()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabAccessErrorReport3()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, local.EabFileHandling);
  }

  private void UseCabIncrementCoArray()
  {
    var useImport = new CabIncrementCoArray.Import();
    var useExport = new CabIncrementCoArray.Export();

    local.AcrossCo.CopyTo(useImport.AcrossCo, MoveAcrossCo1);
    useImport.CollectionsExtract.Assign(local.CollectionsExtract);

    Call(CabIncrementCoArray.Execute, useImport, useExport);

    useExport.AcrossCo.CopyTo(local.AcrossCo, MoveAcrossCo2);
  }

  private void UseCabIncrementRegionArray()
  {
    var useImport = new CabIncrementRegionArray.Import();
    var useExport = new CabIncrementRegionArray.Export();

    useImport.FirstTimeThru.Flag = local.FirstTimeThru.Flag;
    local.AcrossRegion.CopyTo(useImport.AcrossRegion, MoveAcrossRegion1);
    useImport.CollectionsExtract.Assign(local.CollectionsExtract);

    Call(CabIncrementRegionArray.Execute, useImport, useExport);

    useExport.AcrossRegion.CopyTo(local.AcrossRegion, MoveAcrossRegion2);
  }

  private void UseCabIncrementSsArray()
  {
    var useImport = new CabIncrementSsArray.Import();
    var useExport = new CabIncrementSsArray.Export();

    local.AcrossSs.CopyTo(useImport.AcrossSs, MoveAcrossSs1);
    useImport.CollectionsExtract.Assign(local.CollectionsExtract);

    Call(CabIncrementSsArray.Execute, useImport, useExport);

    useExport.AcrossSs.CopyTo(local.AcrossSs, MoveAcrossSs2);
  }

  private void UseCabIncrementStateArray()
  {
    var useImport = new CabIncrementStateArray.Import();
    var useExport = new CabIncrementStateArray.Export();

    useImport.FirstTimeThru.Flag = local.FirstTimeThru.Flag;
    local.AcrossState.CopyTo(useImport.AcrossState, MoveAcrossState1);
    useImport.CollectionsExtract.Assign(local.CollectionsExtract);

    Call(CabIncrementStateArray.Execute, useImport, useExport);

    useExport.AcrossState.CopyTo(local.AcrossState, MoveAcrossState2);
  }

  private void UseCabInitializeArrays1()
  {
    var useImport = new CabInitializeArrays.Import();
    var useExport = new CabInitializeArrays.Export();

    Call(CabInitializeArrays.Execute, useImport, useExport);

    useExport.AcrossCo.CopyTo(local.AcrossCo, MoveAcrossCo3);
  }

  private void UseCabInitializeArrays2()
  {
    var useImport = new CabInitializeArrays.Import();
    var useExport = new CabInitializeArrays.Export();

    Call(CabInitializeArrays.Execute, useImport, useExport);

    useExport.AcrossCo.CopyTo(local.AcrossCo, MoveAcrossCo3);
    useExport.AcrossSs.CopyTo(local.AcrossSs, MoveAcrossSs3);
  }

  private void UseCabInitializeArrays3()
  {
    var useImport = new CabInitializeArrays.Import();
    var useExport = new CabInitializeArrays.Export();

    Call(CabInitializeArrays.Execute, useImport, useExport);

    useExport.AcrossCo.CopyTo(local.AcrossCo, MoveAcrossCo3);
    useExport.AcrossSs.CopyTo(local.AcrossSs, MoveAcrossSs3);
    useExport.AcrossRegion.CopyTo(local.AcrossRegion, MoveAcrossRegion3);
  }

  private void UseEabProduceReport1()
  {
    var useImport = new EabProduceReport.Import();
    var useExport = new EabProduceReport.Export();

    useImport.ReportParms.Assign(local.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabProduceReport2()
  {
    var useImport = new EabProduceReport.Import();
    var useExport = new EabProduceReport.Export();

    useImport.CollectionsExtract.CollectionOfficer =
      export.CollectionsExtract.CollectionOfficer;
    useImport.ReportParms.Assign(local.ReportParms);
    useImport.ReportLiterals.Assign(export.ReportLiterals);
    useImport.TotalCollectionsExtract.Amount1 =
      export.TotalCollectionsExtract.Amount1;
    useImport.TotalCommon.Count = export.TotalCommon.Count;
    useImport.TafTotalCollectionsExtract.Amount1 =
      export.TafTotalCollectionsExtract.Amount1;
    useImport.TafTotalCommon.Count = export.TafTotalCommon.Count;
    useImport.TafCollectionsExtract.Amount1 =
      export.TafCollectionsExtract.Amount1;
    useImport.TafCommon.Count = export.TafCommon.Count;
    useImport.XtafCollectionsExtract.Amount1 =
      export.XtafCollectionsExtract.Amount1;
    useImport.XtafCommon.Count = export.XtafCommon.Count;
    useImport.TafFcCollectionsExtract.Amount1 =
      export.TafFcCollectionsExtract.Amount1;
    useImport.TafFcCommon.Count = export.TafFcCommon.Count;
    useImport.NonTafTotalCollectionsExtract.Amount1 =
      export.NonTafTotalCollectionsExtract.Amount1;
    useImport.NonTafTotalCommon.Count = export.NonTafTotalCommon.Count;
    useImport.NaCollectionsExtract.Amount1 =
      export.NaCollectionsExtract.Amount1;
    useImport.NaCommon.Count = export.NaCommon.Count;
    useImport.PaCollectionsExtract.Amount1 =
      export.PaCollectionsExtract.Amount1;
    useImport.PaCommon.Count = export.PaCommon.Count;
    useImport.StateOnlyTotalCollectionsExtract.Amount1 =
      export.StateOnlyTotalCollectionsExtract.Amount1;
    useImport.StateOnlyTotalCommon.Count = export.StateOnlyTotalCommon.Count;
    useImport.GaFcCollectionsExtract.Amount1 =
      export.GaFcCollectionsExtract.Amount1;
    useImport.GaFcCommon.Count = export.GaFcCommon.Count;
    useImport.MhddCollectionsExtract.Amount1 =
      export.MhddCollectionsExtract.Amount1;
    useImport.MhddCommon.Count = export.MhddCommon.Count;
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabProduceReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabReadExtractFile1()
  {
    var useImport = new EabReadExtractFile.Import();
    var useExport = new EabReadExtractFile.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabReadExtractFile.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabReadExtractFile2()
  {
    var useImport = new EabReadExtractFile.Import();
    var useExport = new EabReadExtractFile.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);
    useExport.CollectionsExtract.Assign(local.CollectionsExtract);

    Call(EabReadExtractFile.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
    local.CollectionsExtract.Assign(useExport.CollectionsExtract);
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// <summary>
    /// A value of CollectionsExtract.
    /// </summary>
    [JsonPropertyName("collectionsExtract")]
    public CollectionsExtract CollectionsExtract
    {
      get => collectionsExtract ??= new();
      set => collectionsExtract = value;
    }

    /// <summary>
    /// A value of ReportLiterals.
    /// </summary>
    [JsonPropertyName("reportLiterals")]
    public ReportLiterals ReportLiterals
    {
      get => reportLiterals ??= new();
      set => reportLiterals = value;
    }

    /// <summary>
    /// A value of TotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("totalCollectionsExtract")]
    public CollectionsExtract TotalCollectionsExtract
    {
      get => totalCollectionsExtract ??= new();
      set => totalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafTotalCollectionsExtract")]
    public CollectionsExtract TafTotalCollectionsExtract
    {
      get => tafTotalCollectionsExtract ??= new();
      set => tafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafCollectionsExtract")]
    public CollectionsExtract TafCollectionsExtract
    {
      get => tafCollectionsExtract ??= new();
      set => tafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of XtafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("xtafCollectionsExtract")]
    public CollectionsExtract XtafCollectionsExtract
    {
      get => xtafCollectionsExtract ??= new();
      set => xtafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafFcCollectionsExtract")]
    public CollectionsExtract TafFcCollectionsExtract
    {
      get => tafFcCollectionsExtract ??= new();
      set => tafFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NonTafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("nonTafTotalCollectionsExtract")]
    public CollectionsExtract NonTafTotalCollectionsExtract
    {
      get => nonTafTotalCollectionsExtract ??= new();
      set => nonTafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("naCollectionsExtract")]
    public CollectionsExtract NaCollectionsExtract
    {
      get => naCollectionsExtract ??= new();
      set => naCollectionsExtract = value;
    }

    /// <summary>
    /// A value of PaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("paCollectionsExtract")]
    public CollectionsExtract PaCollectionsExtract
    {
      get => paCollectionsExtract ??= new();
      set => paCollectionsExtract = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCollectionsExtract")]
    public CollectionsExtract StateOnlyTotalCollectionsExtract
    {
      get => stateOnlyTotalCollectionsExtract ??= new();
      set => stateOnlyTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of GaFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("gaFcCollectionsExtract")]
    public CollectionsExtract GaFcCollectionsExtract
    {
      get => gaFcCollectionsExtract ??= new();
      set => gaFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of MhddCollectionsExtract.
    /// </summary>
    [JsonPropertyName("mhddCollectionsExtract")]
    public CollectionsExtract MhddCollectionsExtract
    {
      get => mhddCollectionsExtract ??= new();
      set => mhddCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TotalCommon.
    /// </summary>
    [JsonPropertyName("totalCommon")]
    public Common TotalCommon
    {
      get => totalCommon ??= new();
      set => totalCommon = value;
    }

    /// <summary>
    /// A value of TafTotalCommon.
    /// </summary>
    [JsonPropertyName("tafTotalCommon")]
    public Common TafTotalCommon
    {
      get => tafTotalCommon ??= new();
      set => tafTotalCommon = value;
    }

    /// <summary>
    /// A value of TafCommon.
    /// </summary>
    [JsonPropertyName("tafCommon")]
    public Common TafCommon
    {
      get => tafCommon ??= new();
      set => tafCommon = value;
    }

    /// <summary>
    /// A value of XtafCommon.
    /// </summary>
    [JsonPropertyName("xtafCommon")]
    public Common XtafCommon
    {
      get => xtafCommon ??= new();
      set => xtafCommon = value;
    }

    /// <summary>
    /// A value of TafFcCommon.
    /// </summary>
    [JsonPropertyName("tafFcCommon")]
    public Common TafFcCommon
    {
      get => tafFcCommon ??= new();
      set => tafFcCommon = value;
    }

    /// <summary>
    /// A value of NonTafTotalCommon.
    /// </summary>
    [JsonPropertyName("nonTafTotalCommon")]
    public Common NonTafTotalCommon
    {
      get => nonTafTotalCommon ??= new();
      set => nonTafTotalCommon = value;
    }

    /// <summary>
    /// A value of NaCommon.
    /// </summary>
    [JsonPropertyName("naCommon")]
    public Common NaCommon
    {
      get => naCommon ??= new();
      set => naCommon = value;
    }

    /// <summary>
    /// A value of PaCommon.
    /// </summary>
    [JsonPropertyName("paCommon")]
    public Common PaCommon
    {
      get => paCommon ??= new();
      set => paCommon = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCommon.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCommon")]
    public Common StateOnlyTotalCommon
    {
      get => stateOnlyTotalCommon ??= new();
      set => stateOnlyTotalCommon = value;
    }

    /// <summary>
    /// A value of GaFcCommon.
    /// </summary>
    [JsonPropertyName("gaFcCommon")]
    public Common GaFcCommon
    {
      get => gaFcCommon ??= new();
      set => gaFcCommon = value;
    }

    /// <summary>
    /// A value of MhddCommon.
    /// </summary>
    [JsonPropertyName("mhddCommon")]
    public Common MhddCommon
    {
      get => mhddCommon ??= new();
      set => mhddCommon = value;
    }

    private CollectionsExtract collectionsExtract;
    private ReportLiterals reportLiterals;
    private CollectionsExtract totalCollectionsExtract;
    private CollectionsExtract tafTotalCollectionsExtract;
    private CollectionsExtract tafCollectionsExtract;
    private CollectionsExtract xtafCollectionsExtract;
    private CollectionsExtract tafFcCollectionsExtract;
    private CollectionsExtract nonTafTotalCollectionsExtract;
    private CollectionsExtract naCollectionsExtract;
    private CollectionsExtract paCollectionsExtract;
    private CollectionsExtract stateOnlyTotalCollectionsExtract;
    private CollectionsExtract gaFcCollectionsExtract;
    private CollectionsExtract mhddCollectionsExtract;
    private Common totalCommon;
    private Common tafTotalCommon;
    private Common tafCommon;
    private Common xtafCommon;
    private Common tafFcCommon;
    private Common nonTafTotalCommon;
    private Common naCommon;
    private Common paCommon;
    private Common stateOnlyTotalCommon;
    private Common gaFcCommon;
    private Common mhddCommon;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of GrandTotal.
    /// </summary>
    [JsonPropertyName("grandTotal")]
    public Common GrandTotal
    {
      get => grandTotal ??= new();
      set => grandTotal = value;
    }

    /// <summary>
    /// A value of Interest.
    /// </summary>
    [JsonPropertyName("interest")]
    public Common Interest
    {
      get => interest ??= new();
      set => interest = value;
    }

    /// <summary>
    /// A value of TotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("totalCollectionsExtract")]
    public CollectionsExtract TotalCollectionsExtract
    {
      get => totalCollectionsExtract ??= new();
      set => totalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafTotalCollectionsExtract")]
    public CollectionsExtract TafTotalCollectionsExtract
    {
      get => tafTotalCollectionsExtract ??= new();
      set => tafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafCollectionsExtract")]
    public CollectionsExtract TafCollectionsExtract
    {
      get => tafCollectionsExtract ??= new();
      set => tafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of XtafCollectionsExtract.
    /// </summary>
    [JsonPropertyName("xtafCollectionsExtract")]
    public CollectionsExtract XtafCollectionsExtract
    {
      get => xtafCollectionsExtract ??= new();
      set => xtafCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TafFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("tafFcCollectionsExtract")]
    public CollectionsExtract TafFcCollectionsExtract
    {
      get => tafFcCollectionsExtract ??= new();
      set => tafFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NonTafTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("nonTafTotalCollectionsExtract")]
    public CollectionsExtract NonTafTotalCollectionsExtract
    {
      get => nonTafTotalCollectionsExtract ??= new();
      set => nonTafTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of NaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("naCollectionsExtract")]
    public CollectionsExtract NaCollectionsExtract
    {
      get => naCollectionsExtract ??= new();
      set => naCollectionsExtract = value;
    }

    /// <summary>
    /// A value of PaCollectionsExtract.
    /// </summary>
    [JsonPropertyName("paCollectionsExtract")]
    public CollectionsExtract PaCollectionsExtract
    {
      get => paCollectionsExtract ??= new();
      set => paCollectionsExtract = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCollectionsExtract.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCollectionsExtract")]
    public CollectionsExtract StateOnlyTotalCollectionsExtract
    {
      get => stateOnlyTotalCollectionsExtract ??= new();
      set => stateOnlyTotalCollectionsExtract = value;
    }

    /// <summary>
    /// A value of GaFcCollectionsExtract.
    /// </summary>
    [JsonPropertyName("gaFcCollectionsExtract")]
    public CollectionsExtract GaFcCollectionsExtract
    {
      get => gaFcCollectionsExtract ??= new();
      set => gaFcCollectionsExtract = value;
    }

    /// <summary>
    /// A value of MhddCollectionsExtract.
    /// </summary>
    [JsonPropertyName("mhddCollectionsExtract")]
    public CollectionsExtract MhddCollectionsExtract
    {
      get => mhddCollectionsExtract ??= new();
      set => mhddCollectionsExtract = value;
    }

    /// <summary>
    /// A value of TotalCommon.
    /// </summary>
    [JsonPropertyName("totalCommon")]
    public Common TotalCommon
    {
      get => totalCommon ??= new();
      set => totalCommon = value;
    }

    /// <summary>
    /// A value of TafTotalCommon.
    /// </summary>
    [JsonPropertyName("tafTotalCommon")]
    public Common TafTotalCommon
    {
      get => tafTotalCommon ??= new();
      set => tafTotalCommon = value;
    }

    /// <summary>
    /// A value of TafCommon.
    /// </summary>
    [JsonPropertyName("tafCommon")]
    public Common TafCommon
    {
      get => tafCommon ??= new();
      set => tafCommon = value;
    }

    /// <summary>
    /// A value of XtafCommon.
    /// </summary>
    [JsonPropertyName("xtafCommon")]
    public Common XtafCommon
    {
      get => xtafCommon ??= new();
      set => xtafCommon = value;
    }

    /// <summary>
    /// A value of TafFcCommon.
    /// </summary>
    [JsonPropertyName("tafFcCommon")]
    public Common TafFcCommon
    {
      get => tafFcCommon ??= new();
      set => tafFcCommon = value;
    }

    /// <summary>
    /// A value of NonTafTotalCommon.
    /// </summary>
    [JsonPropertyName("nonTafTotalCommon")]
    public Common NonTafTotalCommon
    {
      get => nonTafTotalCommon ??= new();
      set => nonTafTotalCommon = value;
    }

    /// <summary>
    /// A value of NaCommon.
    /// </summary>
    [JsonPropertyName("naCommon")]
    public Common NaCommon
    {
      get => naCommon ??= new();
      set => naCommon = value;
    }

    /// <summary>
    /// A value of PaCommon.
    /// </summary>
    [JsonPropertyName("paCommon")]
    public Common PaCommon
    {
      get => paCommon ??= new();
      set => paCommon = value;
    }

    /// <summary>
    /// A value of StateOnlyTotalCommon.
    /// </summary>
    [JsonPropertyName("stateOnlyTotalCommon")]
    public Common StateOnlyTotalCommon
    {
      get => stateOnlyTotalCommon ??= new();
      set => stateOnlyTotalCommon = value;
    }

    /// <summary>
    /// A value of GaFcCommon.
    /// </summary>
    [JsonPropertyName("gaFcCommon")]
    public Common GaFcCommon
    {
      get => gaFcCommon ??= new();
      set => gaFcCommon = value;
    }

    /// <summary>
    /// A value of MhddCommon.
    /// </summary>
    [JsonPropertyName("mhddCommon")]
    public Common MhddCommon
    {
      get => mhddCommon ??= new();
      set => mhddCommon = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CollectionsExtract Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of Eof.
    /// </summary>
    [JsonPropertyName("eof")]
    public Common Eof
    {
      get => eof ??= new();
      set => eof = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of ProcessYyyymm.
    /// </summary>
    [JsonPropertyName("processYyyymm")]
    public DateWorkArea ProcessYyyymm
    {
      get => processYyyymm ??= new();
      set => processYyyymm = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private Common grandTotal;
    private Common interest;
    private CollectionsExtract totalCollectionsExtract;
    private CollectionsExtract tafTotalCollectionsExtract;
    private CollectionsExtract tafCollectionsExtract;
    private CollectionsExtract xtafCollectionsExtract;
    private CollectionsExtract tafFcCollectionsExtract;
    private CollectionsExtract nonTafTotalCollectionsExtract;
    private CollectionsExtract naCollectionsExtract;
    private CollectionsExtract paCollectionsExtract;
    private CollectionsExtract stateOnlyTotalCollectionsExtract;
    private CollectionsExtract gaFcCollectionsExtract;
    private CollectionsExtract mhddCollectionsExtract;
    private Common totalCommon;
    private Common tafTotalCommon;
    private Common tafCommon;
    private Common xtafCommon;
    private Common tafFcCommon;
    private Common nonTafTotalCommon;
    private Common naCommon;
    private Common paCommon;
    private Common stateOnlyTotalCommon;
    private Common gaFcCommon;
    private Common mhddCommon;
    private Common firstTimeThru;
    private ReportParms reportParms;
    private Array<AcrossCoGroup> acrossCo;
    private Array<AcrossSsGroup> acrossSs;
    private Array<AcrossRegionGroup> acrossRegion;
    private Array<AcrossStateGroup> acrossState;
    private CollectionsExtract prev;
    private CollectionsExtract collectionsExtract;
    private EabFileHandling eabFileHandling;
    private Common eof;
    private Common found;
    private DateWorkArea processYyyymm;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

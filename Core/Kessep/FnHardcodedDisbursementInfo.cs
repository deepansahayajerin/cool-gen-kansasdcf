// Program: FN_HARDCODED_DISBURSEMENT_INFO, ID: 371753804, model: 746.
// Short name: SWE00490
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_HARDCODED_DISBURSEMENT_INFO.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will set the hardcoded types, reasons and statuses that 
/// are located in the disbursement management subject area.
/// </para>
/// </summary>
[Serializable]
public partial class FnHardcodedDisbursementInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_HARDCODED_DISBURSEMENT_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnHardcodedDisbursementInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnHardcodedDisbursementInfo.
  /// </summary>
  public FnHardcodedDisbursementInfo(IContext context, Import import,
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
    // ****************************************************************
    // Added new codes and updated others in this ridiculous AB. RK 1/11/98
    // ****************************************************************
    // *****
    // Set the CSE Person number for the state so that we will know when the 
    // state is the obligee.
    // Changed the value from 0000000001 to 000000017O.  RK 1/11/98
    // *****
    export.StateObligee.Number = "000000017O";

    // *****
    // Set Disbursement Transaction Types
    // *****
    export.CollectionDisbursementTransaction.Type1 = "C";
    export.PassthruDisbursementTransaction.Type1 = "P";
    export.DisbursementDisbursementTransaction.Type1 = "D";

    // *****
    // Set Recapture Rule Types
    // *****
    export.Obligor.Type1 = "O";
    export.Default1.Type1 = "D";

    // *****
    // Set Disbursement Suppression Status History Types
    // *****
    export.Person.Type1 = "P";
    export.CollectionType.Type1 = "C";
    export.Automatic.Type1 = "A";

    // *****
    // Set Disbursement Transaction System Genererated IDs
    // *****
    export.CollectionDisbursementTransactionType.SystemGeneratedIdentifier = 1;
    export.DisbursementDisbursementTransactionType.SystemGeneratedIdentifier =
      2;
    export.PassthruDisbursementTransactionType.SystemGeneratedIdentifier = 3;

    // *****
    // Set Disbursement Statuses
    // *****
    // ************************
    // Added in Reversed = 4
    // ************************
    export.Released.SystemGeneratedIdentifier = 1;
    export.Processed.SystemGeneratedIdentifier = 2;
    export.Suppressed.SystemGeneratedIdentifier = 3;
    export.Reversed.SystemGeneratedIdentifier = 4;

    // ************************
    // Set Payment Statuses
    // ************************
    // ****************************************************************
    // Changed the following from old designation to new:
    // stop - 6,7. reisreq - 7,10. reisden - 9,11. reml - 10,9
    // lost - 11,21. can - 12,6. candun - 13,12. outlaw - 14,13.
    // ****************************************************************
    export.Req.SystemGeneratedIdentifier = 1;
    export.Doa.SystemGeneratedIdentifier = 2;
    export.Paid.SystemGeneratedIdentifier = 3;
    export.Ret.SystemGeneratedIdentifier = 4;
    export.Held.SystemGeneratedIdentifier = 5;
    export.Can.SystemGeneratedIdentifier = 6;
    export.Stop.SystemGeneratedIdentifier = 7;
    export.Reis.SystemGeneratedIdentifier = 8;
    export.Reml.SystemGeneratedIdentifier = 9;
    export.Reisreq.SystemGeneratedIdentifier = 10;
    export.Reisden.SystemGeneratedIdentifier = 11;
    export.Candum.SystemGeneratedIdentifier = 12;
    export.Outlaw.SystemGeneratedIdentifier = 13;
    export.Lost.SystemGeneratedIdentifier = 21;

    // *****
    // Set Disbursement(Payment) Method Types
    // *****
    // ****************************************************************
    // Changed the following from old designation to new:
    // warrant - 1,27. Recap - 4,6. off system(formerly central impressed) 6,8.
    // Added Recovery  its set to 7, and took out local impressed 7
    // ****************************************************************
    export.Eft.SystemGeneratedIdentifier = 2;
    export.InterfundVoucher.SystemGeneratedIdentifier = 3;
    export.JournalVoucher.SystemGeneratedIdentifier = 5;
    export.Recapture.SystemGeneratedIdentifier = 6;
    export.Recovery.SystemGeneratedIdentifier = 7;
    export.OffSystem.SystemGeneratedIdentifier = 8;
    export.Warrant.SystemGeneratedIdentifier = 27;

    // *****
    // Set Program Types
    // *****
    export.Af.ProgramCode = "AF";
    export.Na.ProgramCode = "NA";
    export.Fc.ProgramCode = "FC";
    export.Nf.ProgramCode = "NF";

    // *****
    // Set Disbursement Types
    // *****
    // *********************************
    // New codes for Gift.  rk 3/10/99
    // *********************************
    export.AfGcs.SystemGeneratedIdentifier = 320;
    export.FcGcs.SystemGeneratedIdentifier = 321;
    export.NaGcs.SystemGeneratedIdentifier = 322;
    export.NfGcs.SystemGeneratedIdentifier = 323;
    export.AfiGcs.SystemGeneratedIdentifier = 324;
    export.FciGcs.SystemGeneratedIdentifier = 325;
    export.NaiGcs.SystemGeneratedIdentifier = 326;
    export.NcGcs.SystemGeneratedIdentifier = 327;
    export.AfGsp.SystemGeneratedIdentifier = 328;
    export.NaGsp.SystemGeneratedIdentifier = 329;
    export.AfiGsp.SystemGeneratedIdentifier = 330;
    export.NaiGsp.SystemGeneratedIdentifier = 331;
    export.AfGms.SystemGeneratedIdentifier = 332;
    export.FcGms.SystemGeneratedIdentifier = 333;
    export.NaGms.SystemGeneratedIdentifier = 334;
    export.NfGms.SystemGeneratedIdentifier = 335;
    export.AfiGms.SystemGeneratedIdentifier = 336;
    export.FciGms.SystemGeneratedIdentifier = 337;
    export.NaiGms.SystemGeneratedIdentifier = 338;
    export.NcGms.SystemGeneratedIdentifier = 339;
    export.AfGmc.SystemGeneratedIdentifier = 340;
    export.FcGmc.SystemGeneratedIdentifier = 341;
    export.NaGmc.SystemGeneratedIdentifier = 342;
    export.NfGmc.SystemGeneratedIdentifier = 343;
    export.AfiGmc.SystemGeneratedIdentifier = 344;
    export.FciGmc.SystemGeneratedIdentifier = 345;
    export.NaiGmc.SystemGeneratedIdentifier = 346;
    export.NcGmc.SystemGeneratedIdentifier = 347;

    // ****************************************************************
    // New codes put in. Kept together for now until its known for sure they are
    // to be kept and how they're classified. 1/99
    // ****************************************************************
    export.AfVolR.SystemGeneratedIdentifier = 66;
    export.AfAspj.SystemGeneratedIdentifier = 75;
    export.FcUme.SystemGeneratedIdentifier = 76;
    export.FcIume1.SystemGeneratedIdentifier = 77;
    export.NfUme.SystemGeneratedIdentifier = 78;
    export.NfIume1.SystemGeneratedIdentifier = 79;
    export.FcIij.SystemGeneratedIdentifier = 80;
    export.NfIij.SystemGeneratedIdentifier = 81;
    export.FcCrc.SystemGeneratedIdentifier = 82;
    export.FcIcrc1.SystemGeneratedIdentifier = 83;
    export.NfCrc.SystemGeneratedIdentifier = 84;
    export.NfIcrc1.SystemGeneratedIdentifier = 85;
    export.AfImj.SystemGeneratedIdentifier = 86;
    export.AfIspj.SystemGeneratedIdentifier = 87;
    export.FcAmj.SystemGeneratedIdentifier = 88;
    export.FcImj.SystemGeneratedIdentifier = 89;
    export.NaAmj.SystemGeneratedIdentifier = 90;
    export.NaAmjr.SystemGeneratedIdentifier = 91;
    export.NaCspj.SystemGeneratedIdentifier = 92;
    export.NaAspjr.SystemGeneratedIdentifier = 93;
    export.NaImj.SystemGeneratedIdentifier = 94;
    export.NaImjr.SystemGeneratedIdentifier = 95;
    export.NaIspj.SystemGeneratedIdentifier = 96;
    export.NaIspjr.SystemGeneratedIdentifier = 97;
    export.NfAmj.SystemGeneratedIdentifier = 98;
    export.NfImj.SystemGeneratedIdentifier = 99;
    export.NaAcrch.SystemGeneratedIdentifier = 100;
    export.NaAcrhr.SystemGeneratedIdentifier = 107;
    export.NaIcrch1.SystemGeneratedIdentifier = 108;
    export.NaIcrhr.SystemGeneratedIdentifier = 109;
    export.AfAcrch.SystemGeneratedIdentifier = 110;
    export.AfIcrch.SystemGeneratedIdentifier = 111;
    export.FcAcrch.SystemGeneratedIdentifier = 112;
    export.FcIcrch.SystemGeneratedIdentifier = 113;
    export.NfAcrch.SystemGeneratedIdentifier = 114;
    export.NfIcrch.SystemGeneratedIdentifier = 115;
    export.AfAmc.SystemGeneratedIdentifier = 145;
    export.NaIumer1.SystemGeneratedIdentifier = 147;
    export.AfCrc.SystemGeneratedIdentifier = 149;
    export.Gift.SystemGeneratedIdentifier = 152;
    export.NaiIumer.SystemGeneratedIdentifier = 153;
    export.AfiVolR.SystemGeneratedIdentifier = 219;
    export.AfiAspj.SystemGeneratedIdentifier = 228;
    export.FciUme2.SystemGeneratedIdentifier = 229;
    export.FciIume.SystemGeneratedIdentifier = 230;
    export.FciIij.SystemGeneratedIdentifier = 233;
    export.FciCrc2.SystemGeneratedIdentifier = 235;
    export.FciIcrc.SystemGeneratedIdentifier = 236;
    export.AfiImj.SystemGeneratedIdentifier = 239;
    export.AfiIspj.SystemGeneratedIdentifier = 240;
    export.FciAmj.SystemGeneratedIdentifier = 241;
    export.FciImj.SystemGeneratedIdentifier = 242;
    export.NaiAmj.SystemGeneratedIdentifier = 243;
    export.NaiAmjr.SystemGeneratedIdentifier = 244;
    export.NaiCspj.SystemGeneratedIdentifier = 245;
    export.NaiAspjr.SystemGeneratedIdentifier = 246;
    export.NaiImj.SystemGeneratedIdentifier = 247;
    export.NaiImjr.SystemGeneratedIdentifier = 248;
    export.NaiIspj.SystemGeneratedIdentifier = 249;
    export.NaiIspjr.SystemGeneratedIdentifier = 250;
    export.NaiAcrch.SystemGeneratedIdentifier = 253;
    export.NaiAcrhr.SystemGeneratedIdentifier = 260;
    export.NaiIcrch.SystemGeneratedIdentifier = 261;
    export.NaiIcrhr.SystemGeneratedIdentifier = 262;
    export.AfiAcrch.SystemGeneratedIdentifier = 263;
    export.AfiIcrch.SystemGeneratedIdentifier = 264;
    export.FciAcrch.SystemGeneratedIdentifier = 265;
    export.FciIcrch.SystemGeneratedIdentifier = 266;
    export.AfiAmc.SystemGeneratedIdentifier = 285;
    export.NaiIumer.SystemGeneratedIdentifier = 286;
    export.AfiCrc2.SystemGeneratedIdentifier = 287;
    export.AfiCrc2.SystemGeneratedIdentifier = 288;

    // ***************************************************************
    // New NC codes
    // ***************************************************************
    export.NcAcrch.SystemGeneratedIdentifier = 289;
    export.NcAcs.SystemGeneratedIdentifier = 290;
    export.NcAmc.SystemGeneratedIdentifier = 291;
    export.NcAmj.SystemGeneratedIdentifier = 292;
    export.NcAms.SystemGeneratedIdentifier = 293;
    export.NcAaj.SystemGeneratedIdentifier = 294;
    export.NcCcs.SystemGeneratedIdentifier = 295;
    export.NcCmc.SystemGeneratedIdentifier = 296;
    export.NcCms.SystemGeneratedIdentifier = 297;
    export.NcIaj.SystemGeneratedIdentifier = 298;
    export.NcIcrch.SystemGeneratedIdentifier = 299;
    export.NcIcs.SystemGeneratedIdentifier = 300;
    export.NcIij.SystemGeneratedIdentifier = 301;
    export.NcImc.SystemGeneratedIdentifier = 302;
    export.NcImj.SystemGeneratedIdentifier = 303;
    export.NcIms.SystemGeneratedIdentifier = 304;
    export.NcIume.SystemGeneratedIdentifier = 305;
    export.NcUme.SystemGeneratedIdentifier = 306;

    // ***************************************************************
    // New Spousal Arrears codes
    // ***************************************************************
    export.AfAsaj.SystemGeneratedIdentifier = 307;
    export.AfIsaj.SystemGeneratedIdentifier = 308;
    export.NaAsaj.SystemGeneratedIdentifier = 309;
    export.NaIsaj.SystemGeneratedIdentifier = 310;
    export.AfiAsaj.SystemGeneratedIdentifier = 311;
    export.AfiIsaj.SystemGeneratedIdentifier = 312;
    export.NaiAsaj.SystemGeneratedIdentifier = 313;
    export.NaiIsaj.SystemGeneratedIdentifier = 314;
    export.NcIarjr.SystemGeneratedIdentifier = 0;

    // End of new codes
    // ****************************************************************
    // === CHILD SUPPORT ===
    export.AfCcs.SystemGeneratedIdentifier = 1;
    export.AfAcs.SystemGeneratedIdentifier = 2;
    export.AfIcs.SystemGeneratedIdentifier = 3;
    export.NaCcs.SystemGeneratedIdentifier = 4;
    export.NaAcs.SystemGeneratedIdentifier = 5;
    export.NaIcs.SystemGeneratedIdentifier = 6;
    export.NaCcsr.SystemGeneratedIdentifier = 7;
    export.NaAcsr.SystemGeneratedIdentifier = 8;
    export.NaIcsr.SystemGeneratedIdentifier = 9;
    export.FcCcs.SystemGeneratedIdentifier = 101;
    export.FcAcs.SystemGeneratedIdentifier = 102;
    export.FcIcs.SystemGeneratedIdentifier = 103;
    export.NfCcs.SystemGeneratedIdentifier = 104;
    export.NfAcs.SystemGeneratedIdentifier = 105;
    export.NfIcs.SystemGeneratedIdentifier = 106;
    export.AfiCcs.SystemGeneratedIdentifier = 154;
    export.AfiAcs.SystemGeneratedIdentifier = 155;
    export.AfiIcs.SystemGeneratedIdentifier = 156;
    export.NaiCcs.SystemGeneratedIdentifier = 157;
    export.NaiAcs.SystemGeneratedIdentifier = 158;
    export.NaiIcs.SystemGeneratedIdentifier = 159;
    export.NaiCcsr.SystemGeneratedIdentifier = 160;
    export.NaiAcsr.SystemGeneratedIdentifier = 161;
    export.NaiIcsr.SystemGeneratedIdentifier = 162;
    export.FciCcs.SystemGeneratedIdentifier = 254;
    export.FciAcs.SystemGeneratedIdentifier = 255;
    export.FciIcs.SystemGeneratedIdentifier = 256;

    // === SPOUSAL SUPPORT ===
    export.AfCsp.SystemGeneratedIdentifier = 10;
    export.AfAsp.SystemGeneratedIdentifier = 11;
    export.AfIsp.SystemGeneratedIdentifier = 12;
    export.NaCsp.SystemGeneratedIdentifier = 13;
    export.NaAsp.SystemGeneratedIdentifier = 14;
    export.NaIsp.SystemGeneratedIdentifier = 15;
    export.NaCspr.SystemGeneratedIdentifier = 16;
    export.NaAspr.SystemGeneratedIdentifier = 17;
    export.NaIspr.SystemGeneratedIdentifier = 18;
    export.AfiCsp.SystemGeneratedIdentifier = 163;
    export.AfiAsp.SystemGeneratedIdentifier = 164;
    export.AfiIsp.SystemGeneratedIdentifier = 165;
    export.NaiCsp.SystemGeneratedIdentifier = 166;
    export.NaiAsp.SystemGeneratedIdentifier = 167;
    export.NaiIsp.SystemGeneratedIdentifier = 168;
    export.NaiCspr.SystemGeneratedIdentifier = 169;
    export.NaiAspr.SystemGeneratedIdentifier = 170;
    export.NaiIspr.SystemGeneratedIdentifier = 171;

    // === MEDICAL SUPPORT ===
    export.AfCms.SystemGeneratedIdentifier = 19;
    export.AfAms.SystemGeneratedIdentifier = 20;
    export.AfIms.SystemGeneratedIdentifier = 21;
    export.NaCms.SystemGeneratedIdentifier = 22;
    export.NaAms.SystemGeneratedIdentifier = 23;
    export.NaIms.SystemGeneratedIdentifier = 24;
    export.NaCmsr.SystemGeneratedIdentifier = 25;
    export.NaAmsr.SystemGeneratedIdentifier = 26;
    export.NaImsr.SystemGeneratedIdentifier = 27;
    export.FcCms.SystemGeneratedIdentifier = 119;
    export.FcAms.SystemGeneratedIdentifier = 120;
    export.FcIms.SystemGeneratedIdentifier = 121;
    export.NfCms.SystemGeneratedIdentifier = 122;
    export.NfAms.SystemGeneratedIdentifier = 123;
    export.NfIms.SystemGeneratedIdentifier = 124;
    export.AfiCms.SystemGeneratedIdentifier = 172;
    export.AfiAms.SystemGeneratedIdentifier = 173;
    export.AfiIms.SystemGeneratedIdentifier = 174;
    export.NaiCms.SystemGeneratedIdentifier = 175;
    export.NaiAms.SystemGeneratedIdentifier = 176;
    export.NaiIms.SystemGeneratedIdentifier = 177;
    export.NaiCmsr.SystemGeneratedIdentifier = 178;
    export.NaiAmsr.SystemGeneratedIdentifier = 179;
    export.NaiImsr.SystemGeneratedIdentifier = 180;
    export.FciCms.SystemGeneratedIdentifier = 269;
    export.FciAms.SystemGeneratedIdentifier = 270;
    export.FciIms.SystemGeneratedIdentifier = 271;

    // **************************************
    // Changed 29 to af_amj from af_amc
    // **************************************
    // === MEDICAL COST ===
    export.AfCmc.SystemGeneratedIdentifier = 28;
    export.AfAmj.SystemGeneratedIdentifier = 29;
    export.AfImc.SystemGeneratedIdentifier = 30;
    export.NaCmc.SystemGeneratedIdentifier = 31;
    export.NaAmc.SystemGeneratedIdentifier = 32;
    export.NaImc.SystemGeneratedIdentifier = 33;
    export.NaCmcr.SystemGeneratedIdentifier = 34;
    export.NaAmcr.SystemGeneratedIdentifier = 35;
    export.NaImcr.SystemGeneratedIdentifier = 36;
    export.FcCmc.SystemGeneratedIdentifier = 128;
    export.FcAmc.SystemGeneratedIdentifier = 129;
    export.FcImc.SystemGeneratedIdentifier = 130;
    export.NfCmc.SystemGeneratedIdentifier = 131;
    export.NfAmc.SystemGeneratedIdentifier = 132;
    export.NfImc.SystemGeneratedIdentifier = 133;
    export.AfiCmc.SystemGeneratedIdentifier = 181;
    export.AfiAmj.SystemGeneratedIdentifier = 182;
    export.AfiImc.SystemGeneratedIdentifier = 183;
    export.NaiCmc.SystemGeneratedIdentifier = 184;
    export.NaiAmc.SystemGeneratedIdentifier = 185;
    export.NaiImc.SystemGeneratedIdentifier = 186;
    export.NaiCmcr.SystemGeneratedIdentifier = 187;
    export.NaiAmcr.SystemGeneratedIdentifier = 188;
    export.NaiImcr.SystemGeneratedIdentifier = 189;
    export.FciCmc.SystemGeneratedIdentifier = 275;
    export.FciAmc.SystemGeneratedIdentifier = 276;
    export.FciImc.SystemGeneratedIdentifier = 277;

    // ***************************************************************
    // Changed all Arreage Judgement codes from ARRJ to AAJ. And IARJ to IAJ.
    // ***************************************************************
    // === ARREARAGE JUDGEMENT ===
    export.AfAaj.SystemGeneratedIdentifier = 37;
    export.AfIaj.SystemGeneratedIdentifier = 38;
    export.NaAaj.SystemGeneratedIdentifier = 39;
    export.NaIaj.SystemGeneratedIdentifier = 40;

    // ***CHECK BACK ON 41 AND 42***
    export.NaArrjr.SystemGeneratedIdentifier = 41;
    export.NaIarjr.SystemGeneratedIdentifier = 42;
    export.FcAaj.SystemGeneratedIdentifier = 137;
    export.FcIaj.SystemGeneratedIdentifier = 138;
    export.NfAaj.SystemGeneratedIdentifier = 139;
    export.NfIaj.SystemGeneratedIdentifier = 140;
    export.AfiAaj.SystemGeneratedIdentifier = 190;
    export.AfiIaj.SystemGeneratedIdentifier = 191;
    export.NaiAaj.SystemGeneratedIdentifier = 192;
    export.NaiIaj.SystemGeneratedIdentifier = 193;

    // ***CHECK ON TWO BELOW***
    export.NaiArrjr.SystemGeneratedIdentifier = 194;
    export.NaiIarjr.SystemGeneratedIdentifier = 195;
    export.FciAaj.SystemGeneratedIdentifier = 281;
    export.FciIaj.SystemGeneratedIdentifier = 282;

    // === UNINSURED MEDICAL EXPENSES ===
    export.AfUme.SystemGeneratedIdentifier = 43;
    export.AfIume1.SystemGeneratedIdentifier = 44;
    export.NaUme.SystemGeneratedIdentifier = 45;
    export.NaIume1.SystemGeneratedIdentifier = 46;
    export.NaUmer.SystemGeneratedIdentifier = 47;
    export.NaIumer1.SystemGeneratedIdentifier = 48;
    export.AfiUme2.SystemGeneratedIdentifier = 196;
    export.AfiIume.SystemGeneratedIdentifier = 197;
    export.NaiUme2.SystemGeneratedIdentifier = 198;
    export.NaiIume.SystemGeneratedIdentifier = 199;
    export.NaiUmer2.SystemGeneratedIdentifier = 200;
    export.NaiIumer.SystemGeneratedIdentifier = 201;

    // === INTEREST JUDGEMENT ===
    export.AfIij.SystemGeneratedIdentifier = 49;
    export.NaIij.SystemGeneratedIdentifier = 50;
    export.NaIntjr.SystemGeneratedIdentifier = 51;
    export.AfiIij.SystemGeneratedIdentifier = 202;
    export.NaiIij.SystemGeneratedIdentifier = 203;
    export.NaiIntjr.SystemGeneratedIdentifier = 204;

    // === COST OF RAISING CHILD ===
    export.AfCrc.SystemGeneratedIdentifier = 52;
    export.AfIcrc1.SystemGeneratedIdentifier = 53;
    export.NaCrc.SystemGeneratedIdentifier = 54;
    export.NaIcrc1.SystemGeneratedIdentifier = 55;
    export.NaCrcr.SystemGeneratedIdentifier = 56;
    export.NaIcrcr1.SystemGeneratedIdentifier = 57;
    export.AfiCrc2.SystemGeneratedIdentifier = 205;
    export.AfiIcrc.SystemGeneratedIdentifier = 206;
    export.NaiCrc2.SystemGeneratedIdentifier = 207;
    export.NaiIcrc.SystemGeneratedIdentifier = 208;
    export.NaiCrcr2.SystemGeneratedIdentifier = 209;
    export.NaiIcrcr.SystemGeneratedIdentifier = 210;
    export.IvdRc.SystemGeneratedIdentifier = 58;
    export.IrsNeg.SystemGeneratedIdentifier = 59;
    export.BdckRc.SystemGeneratedIdentifier = 60;
    export.MisAr.SystemGeneratedIdentifier = 61;
    export.MisAp.SystemGeneratedIdentifier = 62;
    export.MisNon.SystemGeneratedIdentifier = 63;
    export.AfVol.SystemGeneratedIdentifier = 64;
    export.NaVol.SystemGeneratedIdentifier = 65;
    export.NaVolR.SystemGeneratedIdentifier = 67;
    export.AfiVol.SystemGeneratedIdentifier = 217;
    export.NaiVol.SystemGeneratedIdentifier = 218;
    export.NaiVolR.SystemGeneratedIdentifier = 220;
    export.ApFee.SystemGeneratedIdentifier = 68;
    export.N718b.SystemGeneratedIdentifier = 69;
    export.I718b.SystemGeneratedIdentifier = 70;
    export.Pt.SystemGeneratedIdentifier = 71;
    export.Ptr.SystemGeneratedIdentifier = 72;
    export.CrFee.SystemGeneratedIdentifier = 73;
    export.Coagfee.SystemGeneratedIdentifier = 74;

    // *****
    // Set Disbursement Transaction Relation Reasons
    // *****
    export.IsRelatedTo.SystemGeneratedIdentifier = 1;

    // *****
    // Set CSE Person Relation Reasons
    // *****
    export.DesignatedPayee.SystemGeneratedIdentifier = 1;

    // *****
    // Set Payment Request classifications
    // *****
    export.Refund.Classification = "REF";
    export.Support.Classification = "SUP";
    export.Advancement.Classification = "ADV";
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public DisbursementType Af
    {
      get => af ??= new();
      set => af = value;
    }

    /// <summary>
    /// A value of AfAaj.
    /// </summary>
    [JsonPropertyName("afAaj")]
    public DisbursementType AfAaj
    {
      get => afAaj ??= new();
      set => afAaj = value;
    }

    /// <summary>
    /// A value of AfAcrch.
    /// </summary>
    [JsonPropertyName("afAcrch")]
    public DisbursementType AfAcrch
    {
      get => afAcrch ??= new();
      set => afAcrch = value;
    }

    /// <summary>
    /// A value of AfAcs.
    /// </summary>
    [JsonPropertyName("afAcs")]
    public DisbursementType AfAcs
    {
      get => afAcs ??= new();
      set => afAcs = value;
    }

    /// <summary>
    /// A value of AfAmj.
    /// </summary>
    [JsonPropertyName("afAmj")]
    public DisbursementType AfAmj
    {
      get => afAmj ??= new();
      set => afAmj = value;
    }

    /// <summary>
    /// A value of AfAmc.
    /// </summary>
    [JsonPropertyName("afAmc")]
    public DisbursementType AfAmc
    {
      get => afAmc ??= new();
      set => afAmc = value;
    }

    /// <summary>
    /// A value of AfAmp.
    /// </summary>
    [JsonPropertyName("afAmp")]
    public DisbursementType AfAmp
    {
      get => afAmp ??= new();
      set => afAmp = value;
    }

    /// <summary>
    /// A value of AfAms.
    /// </summary>
    [JsonPropertyName("afAms")]
    public DisbursementType AfAms
    {
      get => afAms ??= new();
      set => afAms = value;
    }

    /// <summary>
    /// A value of AfAsaj.
    /// </summary>
    [JsonPropertyName("afAsaj")]
    public DisbursementType AfAsaj
    {
      get => afAsaj ??= new();
      set => afAsaj = value;
    }

    /// <summary>
    /// A value of AfAspj.
    /// </summary>
    [JsonPropertyName("afAspj")]
    public DisbursementType AfAspj
    {
      get => afAspj ??= new();
      set => afAspj = value;
    }

    /// <summary>
    /// A value of AfAsp.
    /// </summary>
    [JsonPropertyName("afAsp")]
    public DisbursementType AfAsp
    {
      get => afAsp ??= new();
      set => afAsp = value;
    }

    /// <summary>
    /// A value of AfCcs.
    /// </summary>
    [JsonPropertyName("afCcs")]
    public DisbursementType AfCcs
    {
      get => afCcs ??= new();
      set => afCcs = value;
    }

    /// <summary>
    /// A value of AfCmc.
    /// </summary>
    [JsonPropertyName("afCmc")]
    public DisbursementType AfCmc
    {
      get => afCmc ??= new();
      set => afCmc = value;
    }

    /// <summary>
    /// A value of AfCms.
    /// </summary>
    [JsonPropertyName("afCms")]
    public DisbursementType AfCms
    {
      get => afCms ??= new();
      set => afCms = value;
    }

    /// <summary>
    /// A value of AfCrc.
    /// </summary>
    [JsonPropertyName("afCrc")]
    public DisbursementType AfCrc
    {
      get => afCrc ??= new();
      set => afCrc = value;
    }

    /// <summary>
    /// A value of AfCsp.
    /// </summary>
    [JsonPropertyName("afCsp")]
    public DisbursementType AfCsp
    {
      get => afCsp ??= new();
      set => afCsp = value;
    }

    /// <summary>
    /// A value of AfGcs.
    /// </summary>
    [JsonPropertyName("afGcs")]
    public DisbursementType AfGcs
    {
      get => afGcs ??= new();
      set => afGcs = value;
    }

    /// <summary>
    /// A value of AfGmc.
    /// </summary>
    [JsonPropertyName("afGmc")]
    public DisbursementType AfGmc
    {
      get => afGmc ??= new();
      set => afGmc = value;
    }

    /// <summary>
    /// A value of AfGms.
    /// </summary>
    [JsonPropertyName("afGms")]
    public DisbursementType AfGms
    {
      get => afGms ??= new();
      set => afGms = value;
    }

    /// <summary>
    /// A value of AfGsp.
    /// </summary>
    [JsonPropertyName("afGsp")]
    public DisbursementType AfGsp
    {
      get => afGsp ??= new();
      set => afGsp = value;
    }

    /// <summary>
    /// A value of AfIaj.
    /// </summary>
    [JsonPropertyName("afIaj")]
    public DisbursementType AfIaj
    {
      get => afIaj ??= new();
      set => afIaj = value;
    }

    /// <summary>
    /// A value of AfIcrc1.
    /// </summary>
    [JsonPropertyName("afIcrc1")]
    public DisbursementType AfIcrc1
    {
      get => afIcrc1 ??= new();
      set => afIcrc1 = value;
    }

    /// <summary>
    /// A value of AfIcrch.
    /// </summary>
    [JsonPropertyName("afIcrch")]
    public DisbursementType AfIcrch
    {
      get => afIcrch ??= new();
      set => afIcrch = value;
    }

    /// <summary>
    /// A value of AfIcs.
    /// </summary>
    [JsonPropertyName("afIcs")]
    public DisbursementType AfIcs
    {
      get => afIcs ??= new();
      set => afIcs = value;
    }

    /// <summary>
    /// A value of AfIij.
    /// </summary>
    [JsonPropertyName("afIij")]
    public DisbursementType AfIij
    {
      get => afIij ??= new();
      set => afIij = value;
    }

    /// <summary>
    /// A value of AfImc.
    /// </summary>
    [JsonPropertyName("afImc")]
    public DisbursementType AfImc
    {
      get => afImc ??= new();
      set => afImc = value;
    }

    /// <summary>
    /// A value of AfIms.
    /// </summary>
    [JsonPropertyName("afIms")]
    public DisbursementType AfIms
    {
      get => afIms ??= new();
      set => afIms = value;
    }

    /// <summary>
    /// A value of AfInt.
    /// </summary>
    [JsonPropertyName("afInt")]
    public DisbursementType AfInt
    {
      get => afInt ??= new();
      set => afInt = value;
    }

    /// <summary>
    /// A value of AfIsp.
    /// </summary>
    [JsonPropertyName("afIsp")]
    public DisbursementType AfIsp
    {
      get => afIsp ??= new();
      set => afIsp = value;
    }

    /// <summary>
    /// A value of AfIume1.
    /// </summary>
    [JsonPropertyName("afIume1")]
    public DisbursementType AfIume1
    {
      get => afIume1 ??= new();
      set => afIume1 = value;
    }

    /// <summary>
    /// A value of AfUme.
    /// </summary>
    [JsonPropertyName("afUme")]
    public DisbursementType AfUme
    {
      get => afUme ??= new();
      set => afUme = value;
    }

    /// <summary>
    /// A value of AfVol.
    /// </summary>
    [JsonPropertyName("afVol")]
    public DisbursementType AfVol
    {
      get => afVol ??= new();
      set => afVol = value;
    }

    /// <summary>
    /// A value of AfVolR.
    /// </summary>
    [JsonPropertyName("afVolR")]
    public DisbursementType AfVolR
    {
      get => afVolR ??= new();
      set => afVolR = value;
    }

    /// <summary>
    /// A value of AfImj.
    /// </summary>
    [JsonPropertyName("afImj")]
    public DisbursementType AfImj
    {
      get => afImj ??= new();
      set => afImj = value;
    }

    /// <summary>
    /// A value of AfIsaj.
    /// </summary>
    [JsonPropertyName("afIsaj")]
    public DisbursementType AfIsaj
    {
      get => afIsaj ??= new();
      set => afIsaj = value;
    }

    /// <summary>
    /// A value of AfIspj.
    /// </summary>
    [JsonPropertyName("afIspj")]
    public DisbursementType AfIspj
    {
      get => afIspj ??= new();
      set => afIspj = value;
    }

    /// <summary>
    /// A value of AfiAaj.
    /// </summary>
    [JsonPropertyName("afiAaj")]
    public DisbursementType AfiAaj
    {
      get => afiAaj ??= new();
      set => afiAaj = value;
    }

    /// <summary>
    /// A value of AfiAcrch.
    /// </summary>
    [JsonPropertyName("afiAcrch")]
    public DisbursementType AfiAcrch
    {
      get => afiAcrch ??= new();
      set => afiAcrch = value;
    }

    /// <summary>
    /// A value of AfiAcs.
    /// </summary>
    [JsonPropertyName("afiAcs")]
    public DisbursementType AfiAcs
    {
      get => afiAcs ??= new();
      set => afiAcs = value;
    }

    /// <summary>
    /// A value of AfiAmc.
    /// </summary>
    [JsonPropertyName("afiAmc")]
    public DisbursementType AfiAmc
    {
      get => afiAmc ??= new();
      set => afiAmc = value;
    }

    /// <summary>
    /// A value of AfiAmj.
    /// </summary>
    [JsonPropertyName("afiAmj")]
    public DisbursementType AfiAmj
    {
      get => afiAmj ??= new();
      set => afiAmj = value;
    }

    /// <summary>
    /// A value of AfiAmp.
    /// </summary>
    [JsonPropertyName("afiAmp")]
    public DisbursementType AfiAmp
    {
      get => afiAmp ??= new();
      set => afiAmp = value;
    }

    /// <summary>
    /// A value of AfiAms.
    /// </summary>
    [JsonPropertyName("afiAms")]
    public DisbursementType AfiAms
    {
      get => afiAms ??= new();
      set => afiAms = value;
    }

    /// <summary>
    /// A value of AfiAsp.
    /// </summary>
    [JsonPropertyName("afiAsp")]
    public DisbursementType AfiAsp
    {
      get => afiAsp ??= new();
      set => afiAsp = value;
    }

    /// <summary>
    /// A value of AfiAsaj.
    /// </summary>
    [JsonPropertyName("afiAsaj")]
    public DisbursementType AfiAsaj
    {
      get => afiAsaj ??= new();
      set => afiAsaj = value;
    }

    /// <summary>
    /// A value of AfiAspj.
    /// </summary>
    [JsonPropertyName("afiAspj")]
    public DisbursementType AfiAspj
    {
      get => afiAspj ??= new();
      set => afiAspj = value;
    }

    /// <summary>
    /// A value of AfiCcs.
    /// </summary>
    [JsonPropertyName("afiCcs")]
    public DisbursementType AfiCcs
    {
      get => afiCcs ??= new();
      set => afiCcs = value;
    }

    /// <summary>
    /// A value of AfiCmc.
    /// </summary>
    [JsonPropertyName("afiCmc")]
    public DisbursementType AfiCmc
    {
      get => afiCmc ??= new();
      set => afiCmc = value;
    }

    /// <summary>
    /// A value of AfiCms.
    /// </summary>
    [JsonPropertyName("afiCms")]
    public DisbursementType AfiCms
    {
      get => afiCms ??= new();
      set => afiCms = value;
    }

    /// <summary>
    /// A value of AfiCrc2.
    /// </summary>
    [JsonPropertyName("afiCrc2")]
    public DisbursementType AfiCrc2
    {
      get => afiCrc2 ??= new();
      set => afiCrc2 = value;
    }

    /// <summary>
    /// A value of AfiCsp.
    /// </summary>
    [JsonPropertyName("afiCsp")]
    public DisbursementType AfiCsp
    {
      get => afiCsp ??= new();
      set => afiCsp = value;
    }

    /// <summary>
    /// A value of AfiGcs.
    /// </summary>
    [JsonPropertyName("afiGcs")]
    public DisbursementType AfiGcs
    {
      get => afiGcs ??= new();
      set => afiGcs = value;
    }

    /// <summary>
    /// A value of AfiGmc.
    /// </summary>
    [JsonPropertyName("afiGmc")]
    public DisbursementType AfiGmc
    {
      get => afiGmc ??= new();
      set => afiGmc = value;
    }

    /// <summary>
    /// A value of AfiGms.
    /// </summary>
    [JsonPropertyName("afiGms")]
    public DisbursementType AfiGms
    {
      get => afiGms ??= new();
      set => afiGms = value;
    }

    /// <summary>
    /// A value of AfiGsp.
    /// </summary>
    [JsonPropertyName("afiGsp")]
    public DisbursementType AfiGsp
    {
      get => afiGsp ??= new();
      set => afiGsp = value;
    }

    /// <summary>
    /// A value of AfiIaj.
    /// </summary>
    [JsonPropertyName("afiIaj")]
    public DisbursementType AfiIaj
    {
      get => afiIaj ??= new();
      set => afiIaj = value;
    }

    /// <summary>
    /// A value of AfiIcrc.
    /// </summary>
    [JsonPropertyName("afiIcrc")]
    public DisbursementType AfiIcrc
    {
      get => afiIcrc ??= new();
      set => afiIcrc = value;
    }

    /// <summary>
    /// A value of AfiIcs.
    /// </summary>
    [JsonPropertyName("afiIcs")]
    public DisbursementType AfiIcs
    {
      get => afiIcs ??= new();
      set => afiIcs = value;
    }

    /// <summary>
    /// A value of AfiIij.
    /// </summary>
    [JsonPropertyName("afiIij")]
    public DisbursementType AfiIij
    {
      get => afiIij ??= new();
      set => afiIij = value;
    }

    /// <summary>
    /// A value of AfiImc.
    /// </summary>
    [JsonPropertyName("afiImc")]
    public DisbursementType AfiImc
    {
      get => afiImc ??= new();
      set => afiImc = value;
    }

    /// <summary>
    /// A value of AfiInt.
    /// </summary>
    [JsonPropertyName("afiInt")]
    public DisbursementType AfiInt
    {
      get => afiInt ??= new();
      set => afiInt = value;
    }

    /// <summary>
    /// A value of AfiIsp.
    /// </summary>
    [JsonPropertyName("afiIsp")]
    public DisbursementType AfiIsp
    {
      get => afiIsp ??= new();
      set => afiIsp = value;
    }

    /// <summary>
    /// A value of AfiIms.
    /// </summary>
    [JsonPropertyName("afiIms")]
    public DisbursementType AfiIms
    {
      get => afiIms ??= new();
      set => afiIms = value;
    }

    /// <summary>
    /// A value of AfiIume.
    /// </summary>
    [JsonPropertyName("afiIume")]
    public DisbursementType AfiIume
    {
      get => afiIume ??= new();
      set => afiIume = value;
    }

    /// <summary>
    /// A value of AfiUme2.
    /// </summary>
    [JsonPropertyName("afiUme2")]
    public DisbursementType AfiUme2
    {
      get => afiUme2 ??= new();
      set => afiUme2 = value;
    }

    /// <summary>
    /// A value of AfiVol.
    /// </summary>
    [JsonPropertyName("afiVol")]
    public DisbursementType AfiVol
    {
      get => afiVol ??= new();
      set => afiVol = value;
    }

    /// <summary>
    /// A value of AfiVolR.
    /// </summary>
    [JsonPropertyName("afiVolR")]
    public DisbursementType AfiVolR
    {
      get => afiVolR ??= new();
      set => afiVolR = value;
    }

    /// <summary>
    /// A value of AfiIcrch.
    /// </summary>
    [JsonPropertyName("afiIcrch")]
    public DisbursementType AfiIcrch
    {
      get => afiIcrch ??= new();
      set => afiIcrch = value;
    }

    /// <summary>
    /// A value of AfiImj.
    /// </summary>
    [JsonPropertyName("afiImj")]
    public DisbursementType AfiImj
    {
      get => afiImj ??= new();
      set => afiImj = value;
    }

    /// <summary>
    /// A value of AfiIsaj.
    /// </summary>
    [JsonPropertyName("afiIsaj")]
    public DisbursementType AfiIsaj
    {
      get => afiIsaj ??= new();
      set => afiIsaj = value;
    }

    /// <summary>
    /// A value of AfiIspj.
    /// </summary>
    [JsonPropertyName("afiIspj")]
    public DisbursementType AfiIspj
    {
      get => afiIspj ??= new();
      set => afiIspj = value;
    }

    /// <summary>
    /// A value of Fc.
    /// </summary>
    [JsonPropertyName("fc")]
    public DisbursementType Fc
    {
      get => fc ??= new();
      set => fc = value;
    }

    /// <summary>
    /// A value of FcAaj.
    /// </summary>
    [JsonPropertyName("fcAaj")]
    public DisbursementType FcAaj
    {
      get => fcAaj ??= new();
      set => fcAaj = value;
    }

    /// <summary>
    /// A value of FcAmj.
    /// </summary>
    [JsonPropertyName("fcAmj")]
    public DisbursementType FcAmj
    {
      get => fcAmj ??= new();
      set => fcAmj = value;
    }

    /// <summary>
    /// A value of FcAcs.
    /// </summary>
    [JsonPropertyName("fcAcs")]
    public DisbursementType FcAcs
    {
      get => fcAcs ??= new();
      set => fcAcs = value;
    }

    /// <summary>
    /// A value of FcAcrch.
    /// </summary>
    [JsonPropertyName("fcAcrch")]
    public DisbursementType FcAcrch
    {
      get => fcAcrch ??= new();
      set => fcAcrch = value;
    }

    /// <summary>
    /// A value of FcAmc.
    /// </summary>
    [JsonPropertyName("fcAmc")]
    public DisbursementType FcAmc
    {
      get => fcAmc ??= new();
      set => fcAmc = value;
    }

    /// <summary>
    /// A value of FcAms.
    /// </summary>
    [JsonPropertyName("fcAms")]
    public DisbursementType FcAms
    {
      get => fcAms ??= new();
      set => fcAms = value;
    }

    /// <summary>
    /// A value of FcCcs.
    /// </summary>
    [JsonPropertyName("fcCcs")]
    public DisbursementType FcCcs
    {
      get => fcCcs ??= new();
      set => fcCcs = value;
    }

    /// <summary>
    /// A value of FcCms.
    /// </summary>
    [JsonPropertyName("fcCms")]
    public DisbursementType FcCms
    {
      get => fcCms ??= new();
      set => fcCms = value;
    }

    /// <summary>
    /// A value of FcImj.
    /// </summary>
    [JsonPropertyName("fcImj")]
    public DisbursementType FcImj
    {
      get => fcImj ??= new();
      set => fcImj = value;
    }

    /// <summary>
    /// A value of FcIcs.
    /// </summary>
    [JsonPropertyName("fcIcs")]
    public DisbursementType FcIcs
    {
      get => fcIcs ??= new();
      set => fcIcs = value;
    }

    /// <summary>
    /// A value of FcIcrch.
    /// </summary>
    [JsonPropertyName("fcIcrch")]
    public DisbursementType FcIcrch
    {
      get => fcIcrch ??= new();
      set => fcIcrch = value;
    }

    /// <summary>
    /// A value of FcIms.
    /// </summary>
    [JsonPropertyName("fcIms")]
    public DisbursementType FcIms
    {
      get => fcIms ??= new();
      set => fcIms = value;
    }

    /// <summary>
    /// A value of FcCmc.
    /// </summary>
    [JsonPropertyName("fcCmc")]
    public DisbursementType FcCmc
    {
      get => fcCmc ??= new();
      set => fcCmc = value;
    }

    /// <summary>
    /// A value of FcGmc.
    /// </summary>
    [JsonPropertyName("fcGmc")]
    public DisbursementType FcGmc
    {
      get => fcGmc ??= new();
      set => fcGmc = value;
    }

    /// <summary>
    /// A value of FcGms.
    /// </summary>
    [JsonPropertyName("fcGms")]
    public DisbursementType FcGms
    {
      get => fcGms ??= new();
      set => fcGms = value;
    }

    /// <summary>
    /// A value of FcGcs.
    /// </summary>
    [JsonPropertyName("fcGcs")]
    public DisbursementType FcGcs
    {
      get => fcGcs ??= new();
      set => fcGcs = value;
    }

    /// <summary>
    /// A value of FcImc.
    /// </summary>
    [JsonPropertyName("fcImc")]
    public DisbursementType FcImc
    {
      get => fcImc ??= new();
      set => fcImc = value;
    }

    /// <summary>
    /// A value of FcIaj.
    /// </summary>
    [JsonPropertyName("fcIaj")]
    public DisbursementType FcIaj
    {
      get => fcIaj ??= new();
      set => fcIaj = value;
    }

    /// <summary>
    /// A value of FcUme.
    /// </summary>
    [JsonPropertyName("fcUme")]
    public DisbursementType FcUme
    {
      get => fcUme ??= new();
      set => fcUme = value;
    }

    /// <summary>
    /// A value of FcIume1.
    /// </summary>
    [JsonPropertyName("fcIume1")]
    public DisbursementType FcIume1
    {
      get => fcIume1 ??= new();
      set => fcIume1 = value;
    }

    /// <summary>
    /// A value of FcIij.
    /// </summary>
    [JsonPropertyName("fcIij")]
    public DisbursementType FcIij
    {
      get => fcIij ??= new();
      set => fcIij = value;
    }

    /// <summary>
    /// A value of FcCrc.
    /// </summary>
    [JsonPropertyName("fcCrc")]
    public DisbursementType FcCrc
    {
      get => fcCrc ??= new();
      set => fcCrc = value;
    }

    /// <summary>
    /// A value of FcIcrc1.
    /// </summary>
    [JsonPropertyName("fcIcrc1")]
    public DisbursementType FcIcrc1
    {
      get => fcIcrc1 ??= new();
      set => fcIcrc1 = value;
    }

    /// <summary>
    /// A value of FciAaj.
    /// </summary>
    [JsonPropertyName("fciAaj")]
    public DisbursementType FciAaj
    {
      get => fciAaj ??= new();
      set => fciAaj = value;
    }

    /// <summary>
    /// A value of FciAmj.
    /// </summary>
    [JsonPropertyName("fciAmj")]
    public DisbursementType FciAmj
    {
      get => fciAmj ??= new();
      set => fciAmj = value;
    }

    /// <summary>
    /// A value of FciAcrch.
    /// </summary>
    [JsonPropertyName("fciAcrch")]
    public DisbursementType FciAcrch
    {
      get => fciAcrch ??= new();
      set => fciAcrch = value;
    }

    /// <summary>
    /// A value of FciAcs.
    /// </summary>
    [JsonPropertyName("fciAcs")]
    public DisbursementType FciAcs
    {
      get => fciAcs ??= new();
      set => fciAcs = value;
    }

    /// <summary>
    /// A value of FciCcs.
    /// </summary>
    [JsonPropertyName("fciCcs")]
    public DisbursementType FciCcs
    {
      get => fciCcs ??= new();
      set => fciCcs = value;
    }

    /// <summary>
    /// A value of FciCms.
    /// </summary>
    [JsonPropertyName("fciCms")]
    public DisbursementType FciCms
    {
      get => fciCms ??= new();
      set => fciCms = value;
    }

    /// <summary>
    /// A value of FciGmc.
    /// </summary>
    [JsonPropertyName("fciGmc")]
    public DisbursementType FciGmc
    {
      get => fciGmc ??= new();
      set => fciGmc = value;
    }

    /// <summary>
    /// A value of FciGcs.
    /// </summary>
    [JsonPropertyName("fciGcs")]
    public DisbursementType FciGcs
    {
      get => fciGcs ??= new();
      set => fciGcs = value;
    }

    /// <summary>
    /// A value of FciGms.
    /// </summary>
    [JsonPropertyName("fciGms")]
    public DisbursementType FciGms
    {
      get => fciGms ??= new();
      set => fciGms = value;
    }

    /// <summary>
    /// A value of FciIcrch.
    /// </summary>
    [JsonPropertyName("fciIcrch")]
    public DisbursementType FciIcrch
    {
      get => fciIcrch ??= new();
      set => fciIcrch = value;
    }

    /// <summary>
    /// A value of FciImj.
    /// </summary>
    [JsonPropertyName("fciImj")]
    public DisbursementType FciImj
    {
      get => fciImj ??= new();
      set => fciImj = value;
    }

    /// <summary>
    /// A value of FciIcs.
    /// </summary>
    [JsonPropertyName("fciIcs")]
    public DisbursementType FciIcs
    {
      get => fciIcs ??= new();
      set => fciIcs = value;
    }

    /// <summary>
    /// A value of FciAms.
    /// </summary>
    [JsonPropertyName("fciAms")]
    public DisbursementType FciAms
    {
      get => fciAms ??= new();
      set => fciAms = value;
    }

    /// <summary>
    /// A value of FciIms.
    /// </summary>
    [JsonPropertyName("fciIms")]
    public DisbursementType FciIms
    {
      get => fciIms ??= new();
      set => fciIms = value;
    }

    /// <summary>
    /// A value of FciCmc.
    /// </summary>
    [JsonPropertyName("fciCmc")]
    public DisbursementType FciCmc
    {
      get => fciCmc ??= new();
      set => fciCmc = value;
    }

    /// <summary>
    /// A value of FciAmc.
    /// </summary>
    [JsonPropertyName("fciAmc")]
    public DisbursementType FciAmc
    {
      get => fciAmc ??= new();
      set => fciAmc = value;
    }

    /// <summary>
    /// A value of FciImc.
    /// </summary>
    [JsonPropertyName("fciImc")]
    public DisbursementType FciImc
    {
      get => fciImc ??= new();
      set => fciImc = value;
    }

    /// <summary>
    /// A value of FciIaj.
    /// </summary>
    [JsonPropertyName("fciIaj")]
    public DisbursementType FciIaj
    {
      get => fciIaj ??= new();
      set => fciIaj = value;
    }

    /// <summary>
    /// A value of FciUme2.
    /// </summary>
    [JsonPropertyName("fciUme2")]
    public DisbursementType FciUme2
    {
      get => fciUme2 ??= new();
      set => fciUme2 = value;
    }

    /// <summary>
    /// A value of FciIume.
    /// </summary>
    [JsonPropertyName("fciIume")]
    public DisbursementType FciIume
    {
      get => fciIume ??= new();
      set => fciIume = value;
    }

    /// <summary>
    /// A value of FciIij.
    /// </summary>
    [JsonPropertyName("fciIij")]
    public DisbursementType FciIij
    {
      get => fciIij ??= new();
      set => fciIij = value;
    }

    /// <summary>
    /// A value of FciCrc2.
    /// </summary>
    [JsonPropertyName("fciCrc2")]
    public DisbursementType FciCrc2
    {
      get => fciCrc2 ??= new();
      set => fciCrc2 = value;
    }

    /// <summary>
    /// A value of FciIcrc.
    /// </summary>
    [JsonPropertyName("fciIcrc")]
    public DisbursementType FciIcrc
    {
      get => fciIcrc ??= new();
      set => fciIcrc = value;
    }

    /// <summary>
    /// A value of NaGmc.
    /// </summary>
    [JsonPropertyName("naGmc")]
    public DisbursementType NaGmc
    {
      get => naGmc ??= new();
      set => naGmc = value;
    }

    /// <summary>
    /// A value of NaGms.
    /// </summary>
    [JsonPropertyName("naGms")]
    public DisbursementType NaGms
    {
      get => naGms ??= new();
      set => naGms = value;
    }

    /// <summary>
    /// A value of NaGsp.
    /// </summary>
    [JsonPropertyName("naGsp")]
    public DisbursementType NaGsp
    {
      get => naGsp ??= new();
      set => naGsp = value;
    }

    /// <summary>
    /// A value of NaGcs.
    /// </summary>
    [JsonPropertyName("naGcs")]
    public DisbursementType NaGcs
    {
      get => naGcs ??= new();
      set => naGcs = value;
    }

    /// <summary>
    /// A value of NaIsaj.
    /// </summary>
    [JsonPropertyName("naIsaj")]
    public DisbursementType NaIsaj
    {
      get => naIsaj ??= new();
      set => naIsaj = value;
    }

    /// <summary>
    /// A value of NaAsaj.
    /// </summary>
    [JsonPropertyName("naAsaj")]
    public DisbursementType NaAsaj
    {
      get => naAsaj ??= new();
      set => naAsaj = value;
    }

    /// <summary>
    /// A value of NaIcrhr.
    /// </summary>
    [JsonPropertyName("naIcrhr")]
    public DisbursementType NaIcrhr
    {
      get => naIcrhr ??= new();
      set => naIcrhr = value;
    }

    /// <summary>
    /// A value of NaIcrch1.
    /// </summary>
    [JsonPropertyName("naIcrch1")]
    public DisbursementType NaIcrch1
    {
      get => naIcrch1 ??= new();
      set => naIcrch1 = value;
    }

    /// <summary>
    /// A value of NaAcrhr.
    /// </summary>
    [JsonPropertyName("naAcrhr")]
    public DisbursementType NaAcrhr
    {
      get => naAcrhr ??= new();
      set => naAcrhr = value;
    }

    /// <summary>
    /// A value of NaAcrch.
    /// </summary>
    [JsonPropertyName("naAcrch")]
    public DisbursementType NaAcrch
    {
      get => naAcrch ??= new();
      set => naAcrch = value;
    }

    /// <summary>
    /// A value of NaIspjr.
    /// </summary>
    [JsonPropertyName("naIspjr")]
    public DisbursementType NaIspjr
    {
      get => naIspjr ??= new();
      set => naIspjr = value;
    }

    /// <summary>
    /// A value of NaIspj.
    /// </summary>
    [JsonPropertyName("naIspj")]
    public DisbursementType NaIspj
    {
      get => naIspj ??= new();
      set => naIspj = value;
    }

    /// <summary>
    /// A value of NaImjr.
    /// </summary>
    [JsonPropertyName("naImjr")]
    public DisbursementType NaImjr
    {
      get => naImjr ??= new();
      set => naImjr = value;
    }

    /// <summary>
    /// A value of NaImj.
    /// </summary>
    [JsonPropertyName("naImj")]
    public DisbursementType NaImj
    {
      get => naImj ??= new();
      set => naImj = value;
    }

    /// <summary>
    /// A value of NaAspjr.
    /// </summary>
    [JsonPropertyName("naAspjr")]
    public DisbursementType NaAspjr
    {
      get => naAspjr ??= new();
      set => naAspjr = value;
    }

    /// <summary>
    /// A value of NaCspj.
    /// </summary>
    [JsonPropertyName("naCspj")]
    public DisbursementType NaCspj
    {
      get => naCspj ??= new();
      set => naCspj = value;
    }

    /// <summary>
    /// A value of NaAmjr.
    /// </summary>
    [JsonPropertyName("naAmjr")]
    public DisbursementType NaAmjr
    {
      get => naAmjr ??= new();
      set => naAmjr = value;
    }

    /// <summary>
    /// A value of NaAmj.
    /// </summary>
    [JsonPropertyName("naAmj")]
    public DisbursementType NaAmj
    {
      get => naAmj ??= new();
      set => naAmj = value;
    }

    /// <summary>
    /// A value of NaCcs.
    /// </summary>
    [JsonPropertyName("naCcs")]
    public DisbursementType NaCcs
    {
      get => naCcs ??= new();
      set => naCcs = value;
    }

    /// <summary>
    /// A value of NaAcs.
    /// </summary>
    [JsonPropertyName("naAcs")]
    public DisbursementType NaAcs
    {
      get => naAcs ??= new();
      set => naAcs = value;
    }

    /// <summary>
    /// A value of NaIcs.
    /// </summary>
    [JsonPropertyName("naIcs")]
    public DisbursementType NaIcs
    {
      get => naIcs ??= new();
      set => naIcs = value;
    }

    /// <summary>
    /// A value of NaCcsr.
    /// </summary>
    [JsonPropertyName("naCcsr")]
    public DisbursementType NaCcsr
    {
      get => naCcsr ??= new();
      set => naCcsr = value;
    }

    /// <summary>
    /// A value of NaAcsr.
    /// </summary>
    [JsonPropertyName("naAcsr")]
    public DisbursementType NaAcsr
    {
      get => naAcsr ??= new();
      set => naAcsr = value;
    }

    /// <summary>
    /// A value of NaIcsr.
    /// </summary>
    [JsonPropertyName("naIcsr")]
    public DisbursementType NaIcsr
    {
      get => naIcsr ??= new();
      set => naIcsr = value;
    }

    /// <summary>
    /// A value of NaCsp.
    /// </summary>
    [JsonPropertyName("naCsp")]
    public DisbursementType NaCsp
    {
      get => naCsp ??= new();
      set => naCsp = value;
    }

    /// <summary>
    /// A value of NaAsp.
    /// </summary>
    [JsonPropertyName("naAsp")]
    public DisbursementType NaAsp
    {
      get => naAsp ??= new();
      set => naAsp = value;
    }

    /// <summary>
    /// A value of NaIsp.
    /// </summary>
    [JsonPropertyName("naIsp")]
    public DisbursementType NaIsp
    {
      get => naIsp ??= new();
      set => naIsp = value;
    }

    /// <summary>
    /// A value of NaCspr.
    /// </summary>
    [JsonPropertyName("naCspr")]
    public DisbursementType NaCspr
    {
      get => naCspr ??= new();
      set => naCspr = value;
    }

    /// <summary>
    /// A value of NaAspr.
    /// </summary>
    [JsonPropertyName("naAspr")]
    public DisbursementType NaAspr
    {
      get => naAspr ??= new();
      set => naAspr = value;
    }

    /// <summary>
    /// A value of NaIspr.
    /// </summary>
    [JsonPropertyName("naIspr")]
    public DisbursementType NaIspr
    {
      get => naIspr ??= new();
      set => naIspr = value;
    }

    /// <summary>
    /// A value of NaCms.
    /// </summary>
    [JsonPropertyName("naCms")]
    public DisbursementType NaCms
    {
      get => naCms ??= new();
      set => naCms = value;
    }

    /// <summary>
    /// A value of NaAms.
    /// </summary>
    [JsonPropertyName("naAms")]
    public DisbursementType NaAms
    {
      get => naAms ??= new();
      set => naAms = value;
    }

    /// <summary>
    /// A value of NaIms.
    /// </summary>
    [JsonPropertyName("naIms")]
    public DisbursementType NaIms
    {
      get => naIms ??= new();
      set => naIms = value;
    }

    /// <summary>
    /// A value of NaCmsr.
    /// </summary>
    [JsonPropertyName("naCmsr")]
    public DisbursementType NaCmsr
    {
      get => naCmsr ??= new();
      set => naCmsr = value;
    }

    /// <summary>
    /// A value of NaAmsr.
    /// </summary>
    [JsonPropertyName("naAmsr")]
    public DisbursementType NaAmsr
    {
      get => naAmsr ??= new();
      set => naAmsr = value;
    }

    /// <summary>
    /// A value of NaImsr.
    /// </summary>
    [JsonPropertyName("naImsr")]
    public DisbursementType NaImsr
    {
      get => naImsr ??= new();
      set => naImsr = value;
    }

    /// <summary>
    /// A value of NaCmc.
    /// </summary>
    [JsonPropertyName("naCmc")]
    public DisbursementType NaCmc
    {
      get => naCmc ??= new();
      set => naCmc = value;
    }

    /// <summary>
    /// A value of NaAmc.
    /// </summary>
    [JsonPropertyName("naAmc")]
    public DisbursementType NaAmc
    {
      get => naAmc ??= new();
      set => naAmc = value;
    }

    /// <summary>
    /// A value of NaImc.
    /// </summary>
    [JsonPropertyName("naImc")]
    public DisbursementType NaImc
    {
      get => naImc ??= new();
      set => naImc = value;
    }

    /// <summary>
    /// A value of NaCmcr.
    /// </summary>
    [JsonPropertyName("naCmcr")]
    public DisbursementType NaCmcr
    {
      get => naCmcr ??= new();
      set => naCmcr = value;
    }

    /// <summary>
    /// A value of NaAmcr.
    /// </summary>
    [JsonPropertyName("naAmcr")]
    public DisbursementType NaAmcr
    {
      get => naAmcr ??= new();
      set => naAmcr = value;
    }

    /// <summary>
    /// A value of NaImcr.
    /// </summary>
    [JsonPropertyName("naImcr")]
    public DisbursementType NaImcr
    {
      get => naImcr ??= new();
      set => naImcr = value;
    }

    /// <summary>
    /// A value of NaAaj.
    /// </summary>
    [JsonPropertyName("naAaj")]
    public DisbursementType NaAaj
    {
      get => naAaj ??= new();
      set => naAaj = value;
    }

    /// <summary>
    /// A value of NaIaj.
    /// </summary>
    [JsonPropertyName("naIaj")]
    public DisbursementType NaIaj
    {
      get => naIaj ??= new();
      set => naIaj = value;
    }

    /// <summary>
    /// A value of NaArrjr.
    /// </summary>
    [JsonPropertyName("naArrjr")]
    public DisbursementType NaArrjr
    {
      get => naArrjr ??= new();
      set => naArrjr = value;
    }

    /// <summary>
    /// A value of NaIarjr.
    /// </summary>
    [JsonPropertyName("naIarjr")]
    public DisbursementType NaIarjr
    {
      get => naIarjr ??= new();
      set => naIarjr = value;
    }

    /// <summary>
    /// A value of NaUme.
    /// </summary>
    [JsonPropertyName("naUme")]
    public DisbursementType NaUme
    {
      get => naUme ??= new();
      set => naUme = value;
    }

    /// <summary>
    /// A value of NaIume1.
    /// </summary>
    [JsonPropertyName("naIume1")]
    public DisbursementType NaIume1
    {
      get => naIume1 ??= new();
      set => naIume1 = value;
    }

    /// <summary>
    /// A value of NaUmer.
    /// </summary>
    [JsonPropertyName("naUmer")]
    public DisbursementType NaUmer
    {
      get => naUmer ??= new();
      set => naUmer = value;
    }

    /// <summary>
    /// A value of NaIumer1.
    /// </summary>
    [JsonPropertyName("naIumer1")]
    public DisbursementType NaIumer1
    {
      get => naIumer1 ??= new();
      set => naIumer1 = value;
    }

    /// <summary>
    /// A value of NaIij.
    /// </summary>
    [JsonPropertyName("naIij")]
    public DisbursementType NaIij
    {
      get => naIij ??= new();
      set => naIij = value;
    }

    /// <summary>
    /// A value of NaIntjr.
    /// </summary>
    [JsonPropertyName("naIntjr")]
    public DisbursementType NaIntjr
    {
      get => naIntjr ??= new();
      set => naIntjr = value;
    }

    /// <summary>
    /// A value of NaCrc.
    /// </summary>
    [JsonPropertyName("naCrc")]
    public DisbursementType NaCrc
    {
      get => naCrc ??= new();
      set => naCrc = value;
    }

    /// <summary>
    /// A value of NaIcrc1.
    /// </summary>
    [JsonPropertyName("naIcrc1")]
    public DisbursementType NaIcrc1
    {
      get => naIcrc1 ??= new();
      set => naIcrc1 = value;
    }

    /// <summary>
    /// A value of NaCrcr.
    /// </summary>
    [JsonPropertyName("naCrcr")]
    public DisbursementType NaCrcr
    {
      get => naCrcr ??= new();
      set => naCrcr = value;
    }

    /// <summary>
    /// A value of NaIcrcr1.
    /// </summary>
    [JsonPropertyName("naIcrcr1")]
    public DisbursementType NaIcrcr1
    {
      get => naIcrcr1 ??= new();
      set => naIcrcr1 = value;
    }

    /// <summary>
    /// A value of NaVol.
    /// </summary>
    [JsonPropertyName("naVol")]
    public DisbursementType NaVol
    {
      get => naVol ??= new();
      set => naVol = value;
    }

    /// <summary>
    /// A value of NaVolR.
    /// </summary>
    [JsonPropertyName("naVolR")]
    public DisbursementType NaVolR
    {
      get => naVolR ??= new();
      set => naVolR = value;
    }

    /// <summary>
    /// A value of NaCmp.
    /// </summary>
    [JsonPropertyName("naCmp")]
    public DisbursementType NaCmp
    {
      get => naCmp ??= new();
      set => naCmp = value;
    }

    /// <summary>
    /// A value of NaCmpr.
    /// </summary>
    [JsonPropertyName("naCmpr")]
    public DisbursementType NaCmpr
    {
      get => naCmpr ??= new();
      set => naCmpr = value;
    }

    /// <summary>
    /// A value of NaAmp.
    /// </summary>
    [JsonPropertyName("naAmp")]
    public DisbursementType NaAmp
    {
      get => naAmp ??= new();
      set => naAmp = value;
    }

    /// <summary>
    /// A value of NaAmpr.
    /// </summary>
    [JsonPropertyName("naAmpr")]
    public DisbursementType NaAmpr
    {
      get => naAmpr ??= new();
      set => naAmpr = value;
    }

    /// <summary>
    /// A value of NaInt.
    /// </summary>
    [JsonPropertyName("naInt")]
    public DisbursementType NaInt
    {
      get => naInt ??= new();
      set => naInt = value;
    }

    /// <summary>
    /// A value of NaIntr.
    /// </summary>
    [JsonPropertyName("naIntr")]
    public DisbursementType NaIntr
    {
      get => naIntr ??= new();
      set => naIntr = value;
    }

    /// <summary>
    /// A value of NaCrch.
    /// </summary>
    [JsonPropertyName("naCrch")]
    public DisbursementType NaCrch
    {
      get => naCrch ??= new();
      set => naCrch = value;
    }

    /// <summary>
    /// A value of NaFut.
    /// </summary>
    [JsonPropertyName("naFut")]
    public DisbursementType NaFut
    {
      get => naFut ??= new();
      set => naFut = value;
    }

    /// <summary>
    /// A value of Na.
    /// </summary>
    [JsonPropertyName("na")]
    public DisbursementType Na
    {
      get => na ??= new();
      set => na = value;
    }

    /// <summary>
    /// A value of NaiIsaj.
    /// </summary>
    [JsonPropertyName("naiIsaj")]
    public DisbursementType NaiIsaj
    {
      get => naiIsaj ??= new();
      set => naiIsaj = value;
    }

    /// <summary>
    /// A value of NaiGmc.
    /// </summary>
    [JsonPropertyName("naiGmc")]
    public DisbursementType NaiGmc
    {
      get => naiGmc ??= new();
      set => naiGmc = value;
    }

    /// <summary>
    /// A value of NaiGms.
    /// </summary>
    [JsonPropertyName("naiGms")]
    public DisbursementType NaiGms
    {
      get => naiGms ??= new();
      set => naiGms = value;
    }

    /// <summary>
    /// A value of NaiGsp.
    /// </summary>
    [JsonPropertyName("naiGsp")]
    public DisbursementType NaiGsp
    {
      get => naiGsp ??= new();
      set => naiGsp = value;
    }

    /// <summary>
    /// A value of NaiGcs.
    /// </summary>
    [JsonPropertyName("naiGcs")]
    public DisbursementType NaiGcs
    {
      get => naiGcs ??= new();
      set => naiGcs = value;
    }

    /// <summary>
    /// A value of NaiAsaj.
    /// </summary>
    [JsonPropertyName("naiAsaj")]
    public DisbursementType NaiAsaj
    {
      get => naiAsaj ??= new();
      set => naiAsaj = value;
    }

    /// <summary>
    /// A value of NaiIcrhr.
    /// </summary>
    [JsonPropertyName("naiIcrhr")]
    public DisbursementType NaiIcrhr
    {
      get => naiIcrhr ??= new();
      set => naiIcrhr = value;
    }

    /// <summary>
    /// A value of NaiIcrch.
    /// </summary>
    [JsonPropertyName("naiIcrch")]
    public DisbursementType NaiIcrch
    {
      get => naiIcrch ??= new();
      set => naiIcrch = value;
    }

    /// <summary>
    /// A value of NaiAcrhr.
    /// </summary>
    [JsonPropertyName("naiAcrhr")]
    public DisbursementType NaiAcrhr
    {
      get => naiAcrhr ??= new();
      set => naiAcrhr = value;
    }

    /// <summary>
    /// A value of NaiAcrch.
    /// </summary>
    [JsonPropertyName("naiAcrch")]
    public DisbursementType NaiAcrch
    {
      get => naiAcrch ??= new();
      set => naiAcrch = value;
    }

    /// <summary>
    /// A value of NaiIspjr.
    /// </summary>
    [JsonPropertyName("naiIspjr")]
    public DisbursementType NaiIspjr
    {
      get => naiIspjr ??= new();
      set => naiIspjr = value;
    }

    /// <summary>
    /// A value of NaiIspj.
    /// </summary>
    [JsonPropertyName("naiIspj")]
    public DisbursementType NaiIspj
    {
      get => naiIspj ??= new();
      set => naiIspj = value;
    }

    /// <summary>
    /// A value of NaiImjr.
    /// </summary>
    [JsonPropertyName("naiImjr")]
    public DisbursementType NaiImjr
    {
      get => naiImjr ??= new();
      set => naiImjr = value;
    }

    /// <summary>
    /// A value of NaiImj.
    /// </summary>
    [JsonPropertyName("naiImj")]
    public DisbursementType NaiImj
    {
      get => naiImj ??= new();
      set => naiImj = value;
    }

    /// <summary>
    /// A value of NaiAspjr.
    /// </summary>
    [JsonPropertyName("naiAspjr")]
    public DisbursementType NaiAspjr
    {
      get => naiAspjr ??= new();
      set => naiAspjr = value;
    }

    /// <summary>
    /// A value of NaiCspj.
    /// </summary>
    [JsonPropertyName("naiCspj")]
    public DisbursementType NaiCspj
    {
      get => naiCspj ??= new();
      set => naiCspj = value;
    }

    /// <summary>
    /// A value of NaiAmjr.
    /// </summary>
    [JsonPropertyName("naiAmjr")]
    public DisbursementType NaiAmjr
    {
      get => naiAmjr ??= new();
      set => naiAmjr = value;
    }

    /// <summary>
    /// A value of NaiAmj.
    /// </summary>
    [JsonPropertyName("naiAmj")]
    public DisbursementType NaiAmj
    {
      get => naiAmj ??= new();
      set => naiAmj = value;
    }

    /// <summary>
    /// A value of NaiCcs.
    /// </summary>
    [JsonPropertyName("naiCcs")]
    public DisbursementType NaiCcs
    {
      get => naiCcs ??= new();
      set => naiCcs = value;
    }

    /// <summary>
    /// A value of NaiAcs.
    /// </summary>
    [JsonPropertyName("naiAcs")]
    public DisbursementType NaiAcs
    {
      get => naiAcs ??= new();
      set => naiAcs = value;
    }

    /// <summary>
    /// A value of NaiIcs.
    /// </summary>
    [JsonPropertyName("naiIcs")]
    public DisbursementType NaiIcs
    {
      get => naiIcs ??= new();
      set => naiIcs = value;
    }

    /// <summary>
    /// A value of NaiCcsr.
    /// </summary>
    [JsonPropertyName("naiCcsr")]
    public DisbursementType NaiCcsr
    {
      get => naiCcsr ??= new();
      set => naiCcsr = value;
    }

    /// <summary>
    /// A value of NaiAcsr.
    /// </summary>
    [JsonPropertyName("naiAcsr")]
    public DisbursementType NaiAcsr
    {
      get => naiAcsr ??= new();
      set => naiAcsr = value;
    }

    /// <summary>
    /// A value of NaiIcsr.
    /// </summary>
    [JsonPropertyName("naiIcsr")]
    public DisbursementType NaiIcsr
    {
      get => naiIcsr ??= new();
      set => naiIcsr = value;
    }

    /// <summary>
    /// A value of NaiCsp.
    /// </summary>
    [JsonPropertyName("naiCsp")]
    public DisbursementType NaiCsp
    {
      get => naiCsp ??= new();
      set => naiCsp = value;
    }

    /// <summary>
    /// A value of NaiAsp.
    /// </summary>
    [JsonPropertyName("naiAsp")]
    public DisbursementType NaiAsp
    {
      get => naiAsp ??= new();
      set => naiAsp = value;
    }

    /// <summary>
    /// A value of NaiIsp.
    /// </summary>
    [JsonPropertyName("naiIsp")]
    public DisbursementType NaiIsp
    {
      get => naiIsp ??= new();
      set => naiIsp = value;
    }

    /// <summary>
    /// A value of NaiCspr.
    /// </summary>
    [JsonPropertyName("naiCspr")]
    public DisbursementType NaiCspr
    {
      get => naiCspr ??= new();
      set => naiCspr = value;
    }

    /// <summary>
    /// A value of NaiAspr.
    /// </summary>
    [JsonPropertyName("naiAspr")]
    public DisbursementType NaiAspr
    {
      get => naiAspr ??= new();
      set => naiAspr = value;
    }

    /// <summary>
    /// A value of NaiIspr.
    /// </summary>
    [JsonPropertyName("naiIspr")]
    public DisbursementType NaiIspr
    {
      get => naiIspr ??= new();
      set => naiIspr = value;
    }

    /// <summary>
    /// A value of NaiCms.
    /// </summary>
    [JsonPropertyName("naiCms")]
    public DisbursementType NaiCms
    {
      get => naiCms ??= new();
      set => naiCms = value;
    }

    /// <summary>
    /// A value of NaiAms.
    /// </summary>
    [JsonPropertyName("naiAms")]
    public DisbursementType NaiAms
    {
      get => naiAms ??= new();
      set => naiAms = value;
    }

    /// <summary>
    /// A value of NaiIms.
    /// </summary>
    [JsonPropertyName("naiIms")]
    public DisbursementType NaiIms
    {
      get => naiIms ??= new();
      set => naiIms = value;
    }

    /// <summary>
    /// A value of NaiCmsr.
    /// </summary>
    [JsonPropertyName("naiCmsr")]
    public DisbursementType NaiCmsr
    {
      get => naiCmsr ??= new();
      set => naiCmsr = value;
    }

    /// <summary>
    /// A value of NaiAmsr.
    /// </summary>
    [JsonPropertyName("naiAmsr")]
    public DisbursementType NaiAmsr
    {
      get => naiAmsr ??= new();
      set => naiAmsr = value;
    }

    /// <summary>
    /// A value of NaiImsr.
    /// </summary>
    [JsonPropertyName("naiImsr")]
    public DisbursementType NaiImsr
    {
      get => naiImsr ??= new();
      set => naiImsr = value;
    }

    /// <summary>
    /// A value of NaiCmc.
    /// </summary>
    [JsonPropertyName("naiCmc")]
    public DisbursementType NaiCmc
    {
      get => naiCmc ??= new();
      set => naiCmc = value;
    }

    /// <summary>
    /// A value of NaiAmc.
    /// </summary>
    [JsonPropertyName("naiAmc")]
    public DisbursementType NaiAmc
    {
      get => naiAmc ??= new();
      set => naiAmc = value;
    }

    /// <summary>
    /// A value of NaiImc.
    /// </summary>
    [JsonPropertyName("naiImc")]
    public DisbursementType NaiImc
    {
      get => naiImc ??= new();
      set => naiImc = value;
    }

    /// <summary>
    /// A value of NaiCmcr.
    /// </summary>
    [JsonPropertyName("naiCmcr")]
    public DisbursementType NaiCmcr
    {
      get => naiCmcr ??= new();
      set => naiCmcr = value;
    }

    /// <summary>
    /// A value of NaiAmcr.
    /// </summary>
    [JsonPropertyName("naiAmcr")]
    public DisbursementType NaiAmcr
    {
      get => naiAmcr ??= new();
      set => naiAmcr = value;
    }

    /// <summary>
    /// A value of NaiImcr.
    /// </summary>
    [JsonPropertyName("naiImcr")]
    public DisbursementType NaiImcr
    {
      get => naiImcr ??= new();
      set => naiImcr = value;
    }

    /// <summary>
    /// A value of NaiAaj.
    /// </summary>
    [JsonPropertyName("naiAaj")]
    public DisbursementType NaiAaj
    {
      get => naiAaj ??= new();
      set => naiAaj = value;
    }

    /// <summary>
    /// A value of NaiIaj.
    /// </summary>
    [JsonPropertyName("naiIaj")]
    public DisbursementType NaiIaj
    {
      get => naiIaj ??= new();
      set => naiIaj = value;
    }

    /// <summary>
    /// A value of NaiArrjr.
    /// </summary>
    [JsonPropertyName("naiArrjr")]
    public DisbursementType NaiArrjr
    {
      get => naiArrjr ??= new();
      set => naiArrjr = value;
    }

    /// <summary>
    /// A value of NaiIarjr.
    /// </summary>
    [JsonPropertyName("naiIarjr")]
    public DisbursementType NaiIarjr
    {
      get => naiIarjr ??= new();
      set => naiIarjr = value;
    }

    /// <summary>
    /// A value of NaiUme2.
    /// </summary>
    [JsonPropertyName("naiUme2")]
    public DisbursementType NaiUme2
    {
      get => naiUme2 ??= new();
      set => naiUme2 = value;
    }

    /// <summary>
    /// A value of NaiIume.
    /// </summary>
    [JsonPropertyName("naiIume")]
    public DisbursementType NaiIume
    {
      get => naiIume ??= new();
      set => naiIume = value;
    }

    /// <summary>
    /// A value of NaiUmer2.
    /// </summary>
    [JsonPropertyName("naiUmer2")]
    public DisbursementType NaiUmer2
    {
      get => naiUmer2 ??= new();
      set => naiUmer2 = value;
    }

    /// <summary>
    /// A value of NaiIumer.
    /// </summary>
    [JsonPropertyName("naiIumer")]
    public DisbursementType NaiIumer
    {
      get => naiIumer ??= new();
      set => naiIumer = value;
    }

    /// <summary>
    /// A value of NaiIij.
    /// </summary>
    [JsonPropertyName("naiIij")]
    public DisbursementType NaiIij
    {
      get => naiIij ??= new();
      set => naiIij = value;
    }

    /// <summary>
    /// A value of NaiIntjr.
    /// </summary>
    [JsonPropertyName("naiIntjr")]
    public DisbursementType NaiIntjr
    {
      get => naiIntjr ??= new();
      set => naiIntjr = value;
    }

    /// <summary>
    /// A value of NaiCrc2.
    /// </summary>
    [JsonPropertyName("naiCrc2")]
    public DisbursementType NaiCrc2
    {
      get => naiCrc2 ??= new();
      set => naiCrc2 = value;
    }

    /// <summary>
    /// A value of NaiIcrc.
    /// </summary>
    [JsonPropertyName("naiIcrc")]
    public DisbursementType NaiIcrc
    {
      get => naiIcrc ??= new();
      set => naiIcrc = value;
    }

    /// <summary>
    /// A value of NaiCrcr2.
    /// </summary>
    [JsonPropertyName("naiCrcr2")]
    public DisbursementType NaiCrcr2
    {
      get => naiCrcr2 ??= new();
      set => naiCrcr2 = value;
    }

    /// <summary>
    /// A value of NaiIcrcr.
    /// </summary>
    [JsonPropertyName("naiIcrcr")]
    public DisbursementType NaiIcrcr
    {
      get => naiIcrcr ??= new();
      set => naiIcrcr = value;
    }

    /// <summary>
    /// A value of NaiVol.
    /// </summary>
    [JsonPropertyName("naiVol")]
    public DisbursementType NaiVol
    {
      get => naiVol ??= new();
      set => naiVol = value;
    }

    /// <summary>
    /// A value of NaiVolR.
    /// </summary>
    [JsonPropertyName("naiVolR")]
    public DisbursementType NaiVolR
    {
      get => naiVolR ??= new();
      set => naiVolR = value;
    }

    /// <summary>
    /// A value of NaiCmp.
    /// </summary>
    [JsonPropertyName("naiCmp")]
    public DisbursementType NaiCmp
    {
      get => naiCmp ??= new();
      set => naiCmp = value;
    }

    /// <summary>
    /// A value of NaiCmpr.
    /// </summary>
    [JsonPropertyName("naiCmpr")]
    public DisbursementType NaiCmpr
    {
      get => naiCmpr ??= new();
      set => naiCmpr = value;
    }

    /// <summary>
    /// A value of NaiAmp.
    /// </summary>
    [JsonPropertyName("naiAmp")]
    public DisbursementType NaiAmp
    {
      get => naiAmp ??= new();
      set => naiAmp = value;
    }

    /// <summary>
    /// A value of NaiAmpr.
    /// </summary>
    [JsonPropertyName("naiAmpr")]
    public DisbursementType NaiAmpr
    {
      get => naiAmpr ??= new();
      set => naiAmpr = value;
    }

    /// <summary>
    /// A value of NaiInt.
    /// </summary>
    [JsonPropertyName("naiInt")]
    public DisbursementType NaiInt
    {
      get => naiInt ??= new();
      set => naiInt = value;
    }

    /// <summary>
    /// A value of NaiIntr.
    /// </summary>
    [JsonPropertyName("naiIntr")]
    public DisbursementType NaiIntr
    {
      get => naiIntr ??= new();
      set => naiIntr = value;
    }

    /// <summary>
    /// A value of NaiCrch2.
    /// </summary>
    [JsonPropertyName("naiCrch2")]
    public DisbursementType NaiCrch2
    {
      get => naiCrch2 ??= new();
      set => naiCrch2 = value;
    }

    /// <summary>
    /// A value of NaiFut.
    /// </summary>
    [JsonPropertyName("naiFut")]
    public DisbursementType NaiFut
    {
      get => naiFut ??= new();
      set => naiFut = value;
    }

    /// <summary>
    /// A value of NfCms.
    /// </summary>
    [JsonPropertyName("nfCms")]
    public DisbursementType NfCms
    {
      get => nfCms ??= new();
      set => nfCms = value;
    }

    /// <summary>
    /// A value of NfAms.
    /// </summary>
    [JsonPropertyName("nfAms")]
    public DisbursementType NfAms
    {
      get => nfAms ??= new();
      set => nfAms = value;
    }

    /// <summary>
    /// A value of NcIarjr.
    /// </summary>
    [JsonPropertyName("ncIarjr")]
    public DisbursementType NcIarjr
    {
      get => ncIarjr ??= new();
      set => ncIarjr = value;
    }

    /// <summary>
    /// A value of NcIaj.
    /// </summary>
    [JsonPropertyName("ncIaj")]
    public DisbursementType NcIaj
    {
      get => ncIaj ??= new();
      set => ncIaj = value;
    }

    /// <summary>
    /// A value of NcIcrch.
    /// </summary>
    [JsonPropertyName("ncIcrch")]
    public DisbursementType NcIcrch
    {
      get => ncIcrch ??= new();
      set => ncIcrch = value;
    }

    /// <summary>
    /// A value of NcAcrch.
    /// </summary>
    [JsonPropertyName("ncAcrch")]
    public DisbursementType NcAcrch
    {
      get => ncAcrch ??= new();
      set => ncAcrch = value;
    }

    /// <summary>
    /// A value of NcIij.
    /// </summary>
    [JsonPropertyName("ncIij")]
    public DisbursementType NcIij
    {
      get => ncIij ??= new();
      set => ncIij = value;
    }

    /// <summary>
    /// A value of NcIume.
    /// </summary>
    [JsonPropertyName("ncIume")]
    public DisbursementType NcIume
    {
      get => ncIume ??= new();
      set => ncIume = value;
    }

    /// <summary>
    /// A value of NcUme.
    /// </summary>
    [JsonPropertyName("ncUme")]
    public DisbursementType NcUme
    {
      get => ncUme ??= new();
      set => ncUme = value;
    }

    /// <summary>
    /// A value of NcImj.
    /// </summary>
    [JsonPropertyName("ncImj")]
    public DisbursementType NcImj
    {
      get => ncImj ??= new();
      set => ncImj = value;
    }

    /// <summary>
    /// A value of NcGmc.
    /// </summary>
    [JsonPropertyName("ncGmc")]
    public DisbursementType NcGmc
    {
      get => ncGmc ??= new();
      set => ncGmc = value;
    }

    /// <summary>
    /// A value of NcGms.
    /// </summary>
    [JsonPropertyName("ncGms")]
    public DisbursementType NcGms
    {
      get => ncGms ??= new();
      set => ncGms = value;
    }

    /// <summary>
    /// A value of NcGcs.
    /// </summary>
    [JsonPropertyName("ncGcs")]
    public DisbursementType NcGcs
    {
      get => ncGcs ??= new();
      set => ncGcs = value;
    }

    /// <summary>
    /// A value of NcAmj.
    /// </summary>
    [JsonPropertyName("ncAmj")]
    public DisbursementType NcAmj
    {
      get => ncAmj ??= new();
      set => ncAmj = value;
    }

    /// <summary>
    /// A value of NcIarj.
    /// </summary>
    [JsonPropertyName("ncIarj")]
    public DisbursementType NcIarj
    {
      get => ncIarj ??= new();
      set => ncIarj = value;
    }

    /// <summary>
    /// A value of NcAaj.
    /// </summary>
    [JsonPropertyName("ncAaj")]
    public DisbursementType NcAaj
    {
      get => ncAaj ??= new();
      set => ncAaj = value;
    }

    /// <summary>
    /// A value of NcImc.
    /// </summary>
    [JsonPropertyName("ncImc")]
    public DisbursementType NcImc
    {
      get => ncImc ??= new();
      set => ncImc = value;
    }

    /// <summary>
    /// A value of NcAmc.
    /// </summary>
    [JsonPropertyName("ncAmc")]
    public DisbursementType NcAmc
    {
      get => ncAmc ??= new();
      set => ncAmc = value;
    }

    /// <summary>
    /// A value of NcCmc.
    /// </summary>
    [JsonPropertyName("ncCmc")]
    public DisbursementType NcCmc
    {
      get => ncCmc ??= new();
      set => ncCmc = value;
    }

    /// <summary>
    /// A value of NcIms.
    /// </summary>
    [JsonPropertyName("ncIms")]
    public DisbursementType NcIms
    {
      get => ncIms ??= new();
      set => ncIms = value;
    }

    /// <summary>
    /// A value of NcAms.
    /// </summary>
    [JsonPropertyName("ncAms")]
    public DisbursementType NcAms
    {
      get => ncAms ??= new();
      set => ncAms = value;
    }

    /// <summary>
    /// A value of NcCms.
    /// </summary>
    [JsonPropertyName("ncCms")]
    public DisbursementType NcCms
    {
      get => ncCms ??= new();
      set => ncCms = value;
    }

    /// <summary>
    /// A value of NcIcs.
    /// </summary>
    [JsonPropertyName("ncIcs")]
    public DisbursementType NcIcs
    {
      get => ncIcs ??= new();
      set => ncIcs = value;
    }

    /// <summary>
    /// A value of NcAcs.
    /// </summary>
    [JsonPropertyName("ncAcs")]
    public DisbursementType NcAcs
    {
      get => ncAcs ??= new();
      set => ncAcs = value;
    }

    /// <summary>
    /// A value of NcCcs.
    /// </summary>
    [JsonPropertyName("ncCcs")]
    public DisbursementType NcCcs
    {
      get => ncCcs ??= new();
      set => ncCcs = value;
    }

    /// <summary>
    /// A value of NfCcs.
    /// </summary>
    [JsonPropertyName("nfCcs")]
    public DisbursementType NfCcs
    {
      get => nfCcs ??= new();
      set => nfCcs = value;
    }

    /// <summary>
    /// A value of NfAcs.
    /// </summary>
    [JsonPropertyName("nfAcs")]
    public DisbursementType NfAcs
    {
      get => nfAcs ??= new();
      set => nfAcs = value;
    }

    /// <summary>
    /// A value of NfIcs.
    /// </summary>
    [JsonPropertyName("nfIcs")]
    public DisbursementType NfIcs
    {
      get => nfIcs ??= new();
      set => nfIcs = value;
    }

    /// <summary>
    /// A value of NfIms.
    /// </summary>
    [JsonPropertyName("nfIms")]
    public DisbursementType NfIms
    {
      get => nfIms ??= new();
      set => nfIms = value;
    }

    /// <summary>
    /// A value of NfCmc.
    /// </summary>
    [JsonPropertyName("nfCmc")]
    public DisbursementType NfCmc
    {
      get => nfCmc ??= new();
      set => nfCmc = value;
    }

    /// <summary>
    /// A value of NfAmc.
    /// </summary>
    [JsonPropertyName("nfAmc")]
    public DisbursementType NfAmc
    {
      get => nfAmc ??= new();
      set => nfAmc = value;
    }

    /// <summary>
    /// A value of NfImc.
    /// </summary>
    [JsonPropertyName("nfImc")]
    public DisbursementType NfImc
    {
      get => nfImc ??= new();
      set => nfImc = value;
    }

    /// <summary>
    /// A value of NfAaj.
    /// </summary>
    [JsonPropertyName("nfAaj")]
    public DisbursementType NfAaj
    {
      get => nfAaj ??= new();
      set => nfAaj = value;
    }

    /// <summary>
    /// A value of NfIaj.
    /// </summary>
    [JsonPropertyName("nfIaj")]
    public DisbursementType NfIaj
    {
      get => nfIaj ??= new();
      set => nfIaj = value;
    }

    /// <summary>
    /// A value of NfUme.
    /// </summary>
    [JsonPropertyName("nfUme")]
    public DisbursementType NfUme
    {
      get => nfUme ??= new();
      set => nfUme = value;
    }

    /// <summary>
    /// A value of NfIume1.
    /// </summary>
    [JsonPropertyName("nfIume1")]
    public DisbursementType NfIume1
    {
      get => nfIume1 ??= new();
      set => nfIume1 = value;
    }

    /// <summary>
    /// A value of NfIij.
    /// </summary>
    [JsonPropertyName("nfIij")]
    public DisbursementType NfIij
    {
      get => nfIij ??= new();
      set => nfIij = value;
    }

    /// <summary>
    /// A value of NfCrc.
    /// </summary>
    [JsonPropertyName("nfCrc")]
    public DisbursementType NfCrc
    {
      get => nfCrc ??= new();
      set => nfCrc = value;
    }

    /// <summary>
    /// A value of NfIcrc1.
    /// </summary>
    [JsonPropertyName("nfIcrc1")]
    public DisbursementType NfIcrc1
    {
      get => nfIcrc1 ??= new();
      set => nfIcrc1 = value;
    }

    /// <summary>
    /// A value of NfGmc.
    /// </summary>
    [JsonPropertyName("nfGmc")]
    public DisbursementType NfGmc
    {
      get => nfGmc ??= new();
      set => nfGmc = value;
    }

    /// <summary>
    /// A value of NfGms.
    /// </summary>
    [JsonPropertyName("nfGms")]
    public DisbursementType NfGms
    {
      get => nfGms ??= new();
      set => nfGms = value;
    }

    /// <summary>
    /// A value of NfGcs.
    /// </summary>
    [JsonPropertyName("nfGcs")]
    public DisbursementType NfGcs
    {
      get => nfGcs ??= new();
      set => nfGcs = value;
    }

    /// <summary>
    /// A value of Nf.
    /// </summary>
    [JsonPropertyName("nf")]
    public DisbursementType Nf
    {
      get => nf ??= new();
      set => nf = value;
    }

    /// <summary>
    /// A value of NfIcrch.
    /// </summary>
    [JsonPropertyName("nfIcrch")]
    public DisbursementType NfIcrch
    {
      get => nfIcrch ??= new();
      set => nfIcrch = value;
    }

    /// <summary>
    /// A value of NfAcrch.
    /// </summary>
    [JsonPropertyName("nfAcrch")]
    public DisbursementType NfAcrch
    {
      get => nfAcrch ??= new();
      set => nfAcrch = value;
    }

    /// <summary>
    /// A value of NfImj.
    /// </summary>
    [JsonPropertyName("nfImj")]
    public DisbursementType NfImj
    {
      get => nfImj ??= new();
      set => nfImj = value;
    }

    /// <summary>
    /// A value of NfAmj.
    /// </summary>
    [JsonPropertyName("nfAmj")]
    public DisbursementType NfAmj
    {
      get => nfAmj ??= new();
      set => nfAmj = value;
    }

    /// <summary>
    /// A value of NfiCms.
    /// </summary>
    [JsonPropertyName("nfiCms")]
    public DisbursementType NfiCms
    {
      get => nfiCms ??= new();
      set => nfiCms = value;
    }

    /// <summary>
    /// A value of NfiAms.
    /// </summary>
    [JsonPropertyName("nfiAms")]
    public DisbursementType NfiAms
    {
      get => nfiAms ??= new();
      set => nfiAms = value;
    }

    /// <summary>
    /// A value of NfiIms.
    /// </summary>
    [JsonPropertyName("nfiIms")]
    public DisbursementType NfiIms
    {
      get => nfiIms ??= new();
      set => nfiIms = value;
    }

    /// <summary>
    /// A value of NfiCmc.
    /// </summary>
    [JsonPropertyName("nfiCmc")]
    public DisbursementType NfiCmc
    {
      get => nfiCmc ??= new();
      set => nfiCmc = value;
    }

    /// <summary>
    /// A value of NfiAmc.
    /// </summary>
    [JsonPropertyName("nfiAmc")]
    public DisbursementType NfiAmc
    {
      get => nfiAmc ??= new();
      set => nfiAmc = value;
    }

    /// <summary>
    /// A value of NfiImc.
    /// </summary>
    [JsonPropertyName("nfiImc")]
    public DisbursementType NfiImc
    {
      get => nfiImc ??= new();
      set => nfiImc = value;
    }

    /// <summary>
    /// A value of NfiArrj.
    /// </summary>
    [JsonPropertyName("nfiArrj")]
    public DisbursementType NfiArrj
    {
      get => nfiArrj ??= new();
      set => nfiArrj = value;
    }

    /// <summary>
    /// A value of NfiIarj.
    /// </summary>
    [JsonPropertyName("nfiIarj")]
    public DisbursementType NfiIarj
    {
      get => nfiIarj ??= new();
      set => nfiIarj = value;
    }

    /// <summary>
    /// A value of NfiUme2.
    /// </summary>
    [JsonPropertyName("nfiUme2")]
    public DisbursementType NfiUme2
    {
      get => nfiUme2 ??= new();
      set => nfiUme2 = value;
    }

    /// <summary>
    /// A value of NfiIume.
    /// </summary>
    [JsonPropertyName("nfiIume")]
    public DisbursementType NfiIume
    {
      get => nfiIume ??= new();
      set => nfiIume = value;
    }

    /// <summary>
    /// A value of NfiIntj.
    /// </summary>
    [JsonPropertyName("nfiIntj")]
    public DisbursementType NfiIntj
    {
      get => nfiIntj ??= new();
      set => nfiIntj = value;
    }

    /// <summary>
    /// A value of NfiCrc2.
    /// </summary>
    [JsonPropertyName("nfiCrc2")]
    public DisbursementType NfiCrc2
    {
      get => nfiCrc2 ??= new();
      set => nfiCrc2 = value;
    }

    /// <summary>
    /// A value of NfiCcs.
    /// </summary>
    [JsonPropertyName("nfiCcs")]
    public DisbursementType NfiCcs
    {
      get => nfiCcs ??= new();
      set => nfiCcs = value;
    }

    /// <summary>
    /// A value of NfiAcs.
    /// </summary>
    [JsonPropertyName("nfiAcs")]
    public DisbursementType NfiAcs
    {
      get => nfiAcs ??= new();
      set => nfiAcs = value;
    }

    /// <summary>
    /// A value of NfiIcs.
    /// </summary>
    [JsonPropertyName("nfiIcs")]
    public DisbursementType NfiIcs
    {
      get => nfiIcs ??= new();
      set => nfiIcs = value;
    }

    /// <summary>
    /// A value of Group21.
    /// </summary>
    [JsonPropertyName("group21")]
    public Common Group21
    {
      get => group21 ??= new();
      set => group21 = value;
    }

    /// <summary>
    /// A value of Group31.
    /// </summary>
    [JsonPropertyName("group31")]
    public Common Group31
    {
      get => group31 ??= new();
      set => group31 = value;
    }

    /// <summary>
    /// A value of Group41.
    /// </summary>
    [JsonPropertyName("group41")]
    public Common Group41
    {
      get => group41 ??= new();
      set => group41 = value;
    }

    /// <summary>
    /// A value of Group51.
    /// </summary>
    [JsonPropertyName("group51")]
    public Common Group51
    {
      get => group51 ??= new();
      set => group51 = value;
    }

    /// <summary>
    /// A value of Group61.
    /// </summary>
    [JsonPropertyName("group61")]
    public Common Group61
    {
      get => group61 ??= new();
      set => group61 = value;
    }

    /// <summary>
    /// A value of Group71.
    /// </summary>
    [JsonPropertyName("group71")]
    public Common Group71
    {
      get => group71 ??= new();
      set => group71 = value;
    }

    /// <summary>
    /// A value of Group81.
    /// </summary>
    [JsonPropertyName("group81")]
    public Common Group81
    {
      get => group81 ??= new();
      set => group81 = value;
    }

    /// <summary>
    /// A value of CfiIcrc.
    /// </summary>
    [JsonPropertyName("cfiIcrc")]
    public DisbursementType CfiIcrc
    {
      get => cfiIcrc ??= new();
      set => cfiIcrc = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of StateRouting.
    /// </summary>
    [JsonPropertyName("stateRouting")]
    public ElectronicFundTransmission StateRouting
    {
      get => stateRouting ??= new();
      set => stateRouting = value;
    }

    /// <summary>
    /// A value of Group1.
    /// </summary>
    [JsonPropertyName("group1")]
    public Common Group1
    {
      get => group1 ??= new();
      set => group1 = value;
    }

    /// <summary>
    /// A value of Group2.
    /// </summary>
    [JsonPropertyName("group2")]
    public Common Group2
    {
      get => group2 ??= new();
      set => group2 = value;
    }

    /// <summary>
    /// A value of Group3.
    /// </summary>
    [JsonPropertyName("group3")]
    public Common Group3
    {
      get => group3 ??= new();
      set => group3 = value;
    }

    /// <summary>
    /// A value of Group4.
    /// </summary>
    [JsonPropertyName("group4")]
    public Common Group4
    {
      get => group4 ??= new();
      set => group4 = value;
    }

    /// <summary>
    /// A value of Group5.
    /// </summary>
    [JsonPropertyName("group5")]
    public Common Group5
    {
      get => group5 ??= new();
      set => group5 = value;
    }

    /// <summary>
    /// A value of Group6.
    /// </summary>
    [JsonPropertyName("group6")]
    public Common Group6
    {
      get => group6 ??= new();
      set => group6 = value;
    }

    /// <summary>
    /// A value of Group7.
    /// </summary>
    [JsonPropertyName("group7")]
    public Common Group7
    {
      get => group7 ??= new();
      set => group7 = value;
    }

    /// <summary>
    /// A value of Group8.
    /// </summary>
    [JsonPropertyName("group8")]
    public Common Group8
    {
      get => group8 ??= new();
      set => group8 = value;
    }

    /// <summary>
    /// A value of Group9.
    /// </summary>
    [JsonPropertyName("group9")]
    public Common Group9
    {
      get => group9 ??= new();
      set => group9 = value;
    }

    /// <summary>
    /// A value of IvdRc.
    /// </summary>
    [JsonPropertyName("ivdRc")]
    public DisbursementType IvdRc
    {
      get => ivdRc ??= new();
      set => ivdRc = value;
    }

    /// <summary>
    /// A value of IrsNeg.
    /// </summary>
    [JsonPropertyName("irsNeg")]
    public DisbursementType IrsNeg
    {
      get => irsNeg ??= new();
      set => irsNeg = value;
    }

    /// <summary>
    /// A value of BdckRc.
    /// </summary>
    [JsonPropertyName("bdckRc")]
    public DisbursementType BdckRc
    {
      get => bdckRc ??= new();
      set => bdckRc = value;
    }

    /// <summary>
    /// A value of MisAr.
    /// </summary>
    [JsonPropertyName("misAr")]
    public DisbursementType MisAr
    {
      get => misAr ??= new();
      set => misAr = value;
    }

    /// <summary>
    /// A value of MisAp.
    /// </summary>
    [JsonPropertyName("misAp")]
    public DisbursementType MisAp
    {
      get => misAp ??= new();
      set => misAp = value;
    }

    /// <summary>
    /// A value of MisNon.
    /// </summary>
    [JsonPropertyName("misNon")]
    public DisbursementType MisNon
    {
      get => misNon ??= new();
      set => misNon = value;
    }

    /// <summary>
    /// A value of ApFee.
    /// </summary>
    [JsonPropertyName("apFee")]
    public DisbursementType ApFee
    {
      get => apFee ??= new();
      set => apFee = value;
    }

    /// <summary>
    /// A value of N718b.
    /// </summary>
    [JsonPropertyName("n718b")]
    public DisbursementType N718b
    {
      get => n718b ??= new();
      set => n718b = value;
    }

    /// <summary>
    /// A value of I718b.
    /// </summary>
    [JsonPropertyName("i718b")]
    public DisbursementType I718b
    {
      get => i718b ??= new();
      set => i718b = value;
    }

    /// <summary>
    /// A value of Group10.
    /// </summary>
    [JsonPropertyName("group10")]
    public Common Group10
    {
      get => group10 ??= new();
      set => group10 = value;
    }

    /// <summary>
    /// A value of Pt.
    /// </summary>
    [JsonPropertyName("pt")]
    public DisbursementType Pt
    {
      get => pt ??= new();
      set => pt = value;
    }

    /// <summary>
    /// A value of Ptr.
    /// </summary>
    [JsonPropertyName("ptr")]
    public DisbursementType Ptr
    {
      get => ptr ??= new();
      set => ptr = value;
    }

    /// <summary>
    /// A value of CrFee.
    /// </summary>
    [JsonPropertyName("crFee")]
    public DisbursementType CrFee
    {
      get => crFee ??= new();
      set => crFee = value;
    }

    /// <summary>
    /// A value of Coagfee.
    /// </summary>
    [JsonPropertyName("coagfee")]
    public DisbursementType Coagfee
    {
      get => coagfee ??= new();
      set => coagfee = value;
    }

    /// <summary>
    /// A value of AdCmpxxxxxxxxxxxxxxxxxxxxxxxxxx.
    /// </summary>
    [JsonPropertyName("adCmpxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public DisbursementType AdCmpxxxxxxxxxxxxxxxxxxxxxxxxxx
    {
      get => adCmpxxxxxxxxxxxxxxxxxxxxxxxxxx ??= new();
      set => adCmpxxxxxxxxxxxxxxxxxxxxxxxxxx = value;
    }

    /// <summary>
    /// A value of PctUme.
    /// </summary>
    [JsonPropertyName("pctUme")]
    public DisbursementType PctUme
    {
      get => pctUme ??= new();
      set => pctUme = value;
    }

    /// <summary>
    /// A value of VolPmt.
    /// </summary>
    [JsonPropertyName("volPmt")]
    public DisbursementType VolPmt
    {
      get => volPmt ??= new();
      set => volPmt = value;
    }

    /// <summary>
    /// A value of Gift.
    /// </summary>
    [JsonPropertyName("gift")]
    public DisbursementType Gift
    {
      get => gift ??= new();
      set => gift = value;
    }

    /// <summary>
    /// A value of StateObligee.
    /// </summary>
    [JsonPropertyName("stateObligee")]
    public CsePerson StateObligee
    {
      get => stateObligee ??= new();
      set => stateObligee = value;
    }

    /// <summary>
    /// A value of CollectionDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("collectionDisbursementTransaction")]
    public DisbursementTransaction CollectionDisbursementTransaction
    {
      get => collectionDisbursementTransaction ??= new();
      set => collectionDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("passthruDisbursementTransaction")]
    public DisbursementTransaction PassthruDisbursementTransaction
    {
      get => passthruDisbursementTransaction ??= new();
      set => passthruDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementDisbursementTransaction")]
    public DisbursementTransaction DisbursementDisbursementTransaction
    {
      get => disbursementDisbursementTransaction ??= new();
      set => disbursementDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public RecaptureRule Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public RecaptureRule Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of Person.
    /// </summary>
    [JsonPropertyName("person")]
    public DisbSuppressionStatusHistory Person
    {
      get => person ??= new();
      set => person = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public DisbSuppressionStatusHistory CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public DisbSuppressionStatusHistory Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
    }

    /// <summary>
    /// A value of CollectionDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("collectionDisbursementTransactionType")]
    public DisbursementTransactionType CollectionDisbursementTransactionType
    {
      get => collectionDisbursementTransactionType ??= new();
      set => collectionDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of DisbursementDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementDisbursementTransactionType")]
    public DisbursementTransactionType DisbursementDisbursementTransactionType
    {
      get => disbursementDisbursementTransactionType ??= new();
      set => disbursementDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of PassthruDisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("passthruDisbursementTransactionType")]
    public DisbursementTransactionType PassthruDisbursementTransactionType
    {
      get => passthruDisbursementTransactionType ??= new();
      set => passthruDisbursementTransactionType = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public DisbursementStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Suppressed.
    /// </summary>
    [JsonPropertyName("suppressed")]
    public DisbursementStatus Suppressed
    {
      get => suppressed ??= new();
      set => suppressed = value;
    }

    /// <summary>
    /// A value of Reversed.
    /// </summary>
    [JsonPropertyName("reversed")]
    public DisbursementStatus Reversed
    {
      get => reversed ??= new();
      set => reversed = value;
    }

    /// <summary>
    /// A value of Req.
    /// </summary>
    [JsonPropertyName("req")]
    public PaymentStatus Req
    {
      get => req ??= new();
      set => req = value;
    }

    /// <summary>
    /// A value of Doa.
    /// </summary>
    [JsonPropertyName("doa")]
    public PaymentStatus Doa
    {
      get => doa ??= new();
      set => doa = value;
    }

    /// <summary>
    /// A value of Paid.
    /// </summary>
    [JsonPropertyName("paid")]
    public PaymentStatus Paid
    {
      get => paid ??= new();
      set => paid = value;
    }

    /// <summary>
    /// A value of Ret.
    /// </summary>
    [JsonPropertyName("ret")]
    public PaymentStatus Ret
    {
      get => ret ??= new();
      set => ret = value;
    }

    /// <summary>
    /// A value of Held.
    /// </summary>
    [JsonPropertyName("held")]
    public PaymentStatus Held
    {
      get => held ??= new();
      set => held = value;
    }

    /// <summary>
    /// A value of Stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public PaymentStatus Stop
    {
      get => stop ??= new();
      set => stop = value;
    }

    /// <summary>
    /// A value of Reisreq.
    /// </summary>
    [JsonPropertyName("reisreq")]
    public PaymentStatus Reisreq
    {
      get => reisreq ??= new();
      set => reisreq = value;
    }

    /// <summary>
    /// A value of Reis.
    /// </summary>
    [JsonPropertyName("reis")]
    public PaymentStatus Reis
    {
      get => reis ??= new();
      set => reis = value;
    }

    /// <summary>
    /// A value of Reisden.
    /// </summary>
    [JsonPropertyName("reisden")]
    public PaymentStatus Reisden
    {
      get => reisden ??= new();
      set => reisden = value;
    }

    /// <summary>
    /// A value of Reml.
    /// </summary>
    [JsonPropertyName("reml")]
    public PaymentStatus Reml
    {
      get => reml ??= new();
      set => reml = value;
    }

    /// <summary>
    /// A value of Lost.
    /// </summary>
    [JsonPropertyName("lost")]
    public PaymentStatus Lost
    {
      get => lost ??= new();
      set => lost = value;
    }

    /// <summary>
    /// A value of Can.
    /// </summary>
    [JsonPropertyName("can")]
    public PaymentStatus Can
    {
      get => can ??= new();
      set => can = value;
    }

    /// <summary>
    /// A value of Candum.
    /// </summary>
    [JsonPropertyName("candum")]
    public PaymentStatus Candum
    {
      get => candum ??= new();
      set => candum = value;
    }

    /// <summary>
    /// A value of Outlaw.
    /// </summary>
    [JsonPropertyName("outlaw")]
    public PaymentStatus Outlaw
    {
      get => outlaw ??= new();
      set => outlaw = value;
    }

    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentMethodType Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public PaymentMethodType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of InterfundVoucher.
    /// </summary>
    [JsonPropertyName("interfundVoucher")]
    public PaymentMethodType InterfundVoucher
    {
      get => interfundVoucher ??= new();
      set => interfundVoucher = value;
    }

    /// <summary>
    /// A value of Recapture.
    /// </summary>
    [JsonPropertyName("recapture")]
    public PaymentMethodType Recapture
    {
      get => recapture ??= new();
      set => recapture = value;
    }

    /// <summary>
    /// A value of Recovery.
    /// </summary>
    [JsonPropertyName("recovery")]
    public PaymentMethodType Recovery
    {
      get => recovery ??= new();
      set => recovery = value;
    }

    /// <summary>
    /// A value of JournalVoucher.
    /// </summary>
    [JsonPropertyName("journalVoucher")]
    public PaymentMethodType JournalVoucher
    {
      get => journalVoucher ??= new();
      set => journalVoucher = value;
    }

    /// <summary>
    /// A value of OffSystem.
    /// </summary>
    [JsonPropertyName("offSystem")]
    public PaymentMethodType OffSystem
    {
      get => offSystem ??= new();
      set => offSystem = value;
    }

    /// <summary>
    /// A value of ZzzlocalImpressedFund.
    /// </summary>
    [JsonPropertyName("zzzlocalImpressedFund")]
    public PaymentMethodType ZzzlocalImpressedFund
    {
      get => zzzlocalImpressedFund ??= new();
      set => zzzlocalImpressedFund = value;
    }

    /// <summary>
    /// A value of IsRelatedTo.
    /// </summary>
    [JsonPropertyName("isRelatedTo")]
    public DisbursementTranRlnRsn IsRelatedTo
    {
      get => isRelatedTo ??= new();
      set => isRelatedTo = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonRlnRsn DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of Refund.
    /// </summary>
    [JsonPropertyName("refund")]
    public PaymentRequest Refund
    {
      get => refund ??= new();
      set => refund = value;
    }

    /// <summary>
    /// A value of Support.
    /// </summary>
    [JsonPropertyName("support")]
    public PaymentRequest Support
    {
      get => support ??= new();
      set => support = value;
    }

    /// <summary>
    /// A value of Advancement.
    /// </summary>
    [JsonPropertyName("advancement")]
    public PaymentRequest Advancement
    {
      get => advancement ??= new();
      set => advancement = value;
    }

    private DisbursementType af;
    private DisbursementType afAaj;
    private DisbursementType afAcrch;
    private DisbursementType afAcs;
    private DisbursementType afAmj;
    private DisbursementType afAmc;
    private DisbursementType afAmp;
    private DisbursementType afAms;
    private DisbursementType afAsaj;
    private DisbursementType afAspj;
    private DisbursementType afAsp;
    private DisbursementType afCcs;
    private DisbursementType afCmc;
    private DisbursementType afCms;
    private DisbursementType afCrc;
    private DisbursementType afCsp;
    private DisbursementType afGcs;
    private DisbursementType afGmc;
    private DisbursementType afGms;
    private DisbursementType afGsp;
    private DisbursementType afIaj;
    private DisbursementType afIcrc1;
    private DisbursementType afIcrch;
    private DisbursementType afIcs;
    private DisbursementType afIij;
    private DisbursementType afImc;
    private DisbursementType afIms;
    private DisbursementType afInt;
    private DisbursementType afIsp;
    private DisbursementType afIume1;
    private DisbursementType afUme;
    private DisbursementType afVol;
    private DisbursementType afVolR;
    private DisbursementType afImj;
    private DisbursementType afIsaj;
    private DisbursementType afIspj;
    private DisbursementType afiAaj;
    private DisbursementType afiAcrch;
    private DisbursementType afiAcs;
    private DisbursementType afiAmc;
    private DisbursementType afiAmj;
    private DisbursementType afiAmp;
    private DisbursementType afiAms;
    private DisbursementType afiAsp;
    private DisbursementType afiAsaj;
    private DisbursementType afiAspj;
    private DisbursementType afiCcs;
    private DisbursementType afiCmc;
    private DisbursementType afiCms;
    private DisbursementType afiCrc2;
    private DisbursementType afiCsp;
    private DisbursementType afiGcs;
    private DisbursementType afiGmc;
    private DisbursementType afiGms;
    private DisbursementType afiGsp;
    private DisbursementType afiIaj;
    private DisbursementType afiIcrc;
    private DisbursementType afiIcs;
    private DisbursementType afiIij;
    private DisbursementType afiImc;
    private DisbursementType afiInt;
    private DisbursementType afiIsp;
    private DisbursementType afiIms;
    private DisbursementType afiIume;
    private DisbursementType afiUme2;
    private DisbursementType afiVol;
    private DisbursementType afiVolR;
    private DisbursementType afiIcrch;
    private DisbursementType afiImj;
    private DisbursementType afiIsaj;
    private DisbursementType afiIspj;
    private DisbursementType fc;
    private DisbursementType fcAaj;
    private DisbursementType fcAmj;
    private DisbursementType fcAcs;
    private DisbursementType fcAcrch;
    private DisbursementType fcAmc;
    private DisbursementType fcAms;
    private DisbursementType fcCcs;
    private DisbursementType fcCms;
    private DisbursementType fcImj;
    private DisbursementType fcIcs;
    private DisbursementType fcIcrch;
    private DisbursementType fcIms;
    private DisbursementType fcCmc;
    private DisbursementType fcGmc;
    private DisbursementType fcGms;
    private DisbursementType fcGcs;
    private DisbursementType fcImc;
    private DisbursementType fcIaj;
    private DisbursementType fcUme;
    private DisbursementType fcIume1;
    private DisbursementType fcIij;
    private DisbursementType fcCrc;
    private DisbursementType fcIcrc1;
    private DisbursementType fciAaj;
    private DisbursementType fciAmj;
    private DisbursementType fciAcrch;
    private DisbursementType fciAcs;
    private DisbursementType fciCcs;
    private DisbursementType fciCms;
    private DisbursementType fciGmc;
    private DisbursementType fciGcs;
    private DisbursementType fciGms;
    private DisbursementType fciIcrch;
    private DisbursementType fciImj;
    private DisbursementType fciIcs;
    private DisbursementType fciAms;
    private DisbursementType fciIms;
    private DisbursementType fciCmc;
    private DisbursementType fciAmc;
    private DisbursementType fciImc;
    private DisbursementType fciIaj;
    private DisbursementType fciUme2;
    private DisbursementType fciIume;
    private DisbursementType fciIij;
    private DisbursementType fciCrc2;
    private DisbursementType fciIcrc;
    private DisbursementType naGmc;
    private DisbursementType naGms;
    private DisbursementType naGsp;
    private DisbursementType naGcs;
    private DisbursementType naIsaj;
    private DisbursementType naAsaj;
    private DisbursementType naIcrhr;
    private DisbursementType naIcrch1;
    private DisbursementType naAcrhr;
    private DisbursementType naAcrch;
    private DisbursementType naIspjr;
    private DisbursementType naIspj;
    private DisbursementType naImjr;
    private DisbursementType naImj;
    private DisbursementType naAspjr;
    private DisbursementType naCspj;
    private DisbursementType naAmjr;
    private DisbursementType naAmj;
    private DisbursementType naCcs;
    private DisbursementType naAcs;
    private DisbursementType naIcs;
    private DisbursementType naCcsr;
    private DisbursementType naAcsr;
    private DisbursementType naIcsr;
    private DisbursementType naCsp;
    private DisbursementType naAsp;
    private DisbursementType naIsp;
    private DisbursementType naCspr;
    private DisbursementType naAspr;
    private DisbursementType naIspr;
    private DisbursementType naCms;
    private DisbursementType naAms;
    private DisbursementType naIms;
    private DisbursementType naCmsr;
    private DisbursementType naAmsr;
    private DisbursementType naImsr;
    private DisbursementType naCmc;
    private DisbursementType naAmc;
    private DisbursementType naImc;
    private DisbursementType naCmcr;
    private DisbursementType naAmcr;
    private DisbursementType naImcr;
    private DisbursementType naAaj;
    private DisbursementType naIaj;
    private DisbursementType naArrjr;
    private DisbursementType naIarjr;
    private DisbursementType naUme;
    private DisbursementType naIume1;
    private DisbursementType naUmer;
    private DisbursementType naIumer1;
    private DisbursementType naIij;
    private DisbursementType naIntjr;
    private DisbursementType naCrc;
    private DisbursementType naIcrc1;
    private DisbursementType naCrcr;
    private DisbursementType naIcrcr1;
    private DisbursementType naVol;
    private DisbursementType naVolR;
    private DisbursementType naCmp;
    private DisbursementType naCmpr;
    private DisbursementType naAmp;
    private DisbursementType naAmpr;
    private DisbursementType naInt;
    private DisbursementType naIntr;
    private DisbursementType naCrch;
    private DisbursementType naFut;
    private DisbursementType na;
    private DisbursementType naiIsaj;
    private DisbursementType naiGmc;
    private DisbursementType naiGms;
    private DisbursementType naiGsp;
    private DisbursementType naiGcs;
    private DisbursementType naiAsaj;
    private DisbursementType naiIcrhr;
    private DisbursementType naiIcrch;
    private DisbursementType naiAcrhr;
    private DisbursementType naiAcrch;
    private DisbursementType naiIspjr;
    private DisbursementType naiIspj;
    private DisbursementType naiImjr;
    private DisbursementType naiImj;
    private DisbursementType naiAspjr;
    private DisbursementType naiCspj;
    private DisbursementType naiAmjr;
    private DisbursementType naiAmj;
    private DisbursementType naiCcs;
    private DisbursementType naiAcs;
    private DisbursementType naiIcs;
    private DisbursementType naiCcsr;
    private DisbursementType naiAcsr;
    private DisbursementType naiIcsr;
    private DisbursementType naiCsp;
    private DisbursementType naiAsp;
    private DisbursementType naiIsp;
    private DisbursementType naiCspr;
    private DisbursementType naiAspr;
    private DisbursementType naiIspr;
    private DisbursementType naiCms;
    private DisbursementType naiAms;
    private DisbursementType naiIms;
    private DisbursementType naiCmsr;
    private DisbursementType naiAmsr;
    private DisbursementType naiImsr;
    private DisbursementType naiCmc;
    private DisbursementType naiAmc;
    private DisbursementType naiImc;
    private DisbursementType naiCmcr;
    private DisbursementType naiAmcr;
    private DisbursementType naiImcr;
    private DisbursementType naiAaj;
    private DisbursementType naiIaj;
    private DisbursementType naiArrjr;
    private DisbursementType naiIarjr;
    private DisbursementType naiUme2;
    private DisbursementType naiIume;
    private DisbursementType naiUmer2;
    private DisbursementType naiIumer;
    private DisbursementType naiIij;
    private DisbursementType naiIntjr;
    private DisbursementType naiCrc2;
    private DisbursementType naiIcrc;
    private DisbursementType naiCrcr2;
    private DisbursementType naiIcrcr;
    private DisbursementType naiVol;
    private DisbursementType naiVolR;
    private DisbursementType naiCmp;
    private DisbursementType naiCmpr;
    private DisbursementType naiAmp;
    private DisbursementType naiAmpr;
    private DisbursementType naiInt;
    private DisbursementType naiIntr;
    private DisbursementType naiCrch2;
    private DisbursementType naiFut;
    private DisbursementType nfCms;
    private DisbursementType nfAms;
    private DisbursementType ncIarjr;
    private DisbursementType ncIaj;
    private DisbursementType ncIcrch;
    private DisbursementType ncAcrch;
    private DisbursementType ncIij;
    private DisbursementType ncIume;
    private DisbursementType ncUme;
    private DisbursementType ncImj;
    private DisbursementType ncGmc;
    private DisbursementType ncGms;
    private DisbursementType ncGcs;
    private DisbursementType ncAmj;
    private DisbursementType ncIarj;
    private DisbursementType ncAaj;
    private DisbursementType ncImc;
    private DisbursementType ncAmc;
    private DisbursementType ncCmc;
    private DisbursementType ncIms;
    private DisbursementType ncAms;
    private DisbursementType ncCms;
    private DisbursementType ncIcs;
    private DisbursementType ncAcs;
    private DisbursementType ncCcs;
    private DisbursementType nfCcs;
    private DisbursementType nfAcs;
    private DisbursementType nfIcs;
    private DisbursementType nfIms;
    private DisbursementType nfCmc;
    private DisbursementType nfAmc;
    private DisbursementType nfImc;
    private DisbursementType nfAaj;
    private DisbursementType nfIaj;
    private DisbursementType nfUme;
    private DisbursementType nfIume1;
    private DisbursementType nfIij;
    private DisbursementType nfCrc;
    private DisbursementType nfIcrc1;
    private DisbursementType nfGmc;
    private DisbursementType nfGms;
    private DisbursementType nfGcs;
    private DisbursementType nf;
    private DisbursementType nfIcrch;
    private DisbursementType nfAcrch;
    private DisbursementType nfImj;
    private DisbursementType nfAmj;
    private DisbursementType nfiCms;
    private DisbursementType nfiAms;
    private DisbursementType nfiIms;
    private DisbursementType nfiCmc;
    private DisbursementType nfiAmc;
    private DisbursementType nfiImc;
    private DisbursementType nfiArrj;
    private DisbursementType nfiIarj;
    private DisbursementType nfiUme2;
    private DisbursementType nfiIume;
    private DisbursementType nfiIntj;
    private DisbursementType nfiCrc2;
    private DisbursementType nfiCcs;
    private DisbursementType nfiAcs;
    private DisbursementType nfiIcs;
    private Common group21;
    private Common group31;
    private Common group41;
    private Common group51;
    private Common group61;
    private Common group71;
    private Common group81;
    private DisbursementType cfiIcrc;
    private DisbursementType disbursementType;
    private ElectronicFundTransmission stateRouting;
    private Common group1;
    private Common group2;
    private Common group3;
    private Common group4;
    private Common group5;
    private Common group6;
    private Common group7;
    private Common group8;
    private Common group9;
    private DisbursementType ivdRc;
    private DisbursementType irsNeg;
    private DisbursementType bdckRc;
    private DisbursementType misAr;
    private DisbursementType misAp;
    private DisbursementType misNon;
    private DisbursementType apFee;
    private DisbursementType n718b;
    private DisbursementType i718b;
    private Common group10;
    private DisbursementType pt;
    private DisbursementType ptr;
    private DisbursementType crFee;
    private DisbursementType coagfee;
    private DisbursementType adCmpxxxxxxxxxxxxxxxxxxxxxxxxxx;
    private DisbursementType pctUme;
    private DisbursementType volPmt;
    private DisbursementType gift;
    private CsePerson stateObligee;
    private DisbursementTransaction collectionDisbursementTransaction;
    private DisbursementTransaction passthruDisbursementTransaction;
    private DisbursementTransaction disbursementDisbursementTransaction;
    private RecaptureRule obligor;
    private RecaptureRule default1;
    private DisbSuppressionStatusHistory person;
    private DisbSuppressionStatusHistory collectionType;
    private DisbSuppressionStatusHistory automatic;
    private DisbursementTransactionType collectionDisbursementTransactionType;
    private DisbursementTransactionType disbursementDisbursementTransactionType;
    private DisbursementTransactionType passthruDisbursementTransactionType;
    private DisbursementStatus released;
    private DisbursementStatus processed;
    private DisbursementStatus suppressed;
    private DisbursementStatus reversed;
    private PaymentStatus req;
    private PaymentStatus doa;
    private PaymentStatus paid;
    private PaymentStatus ret;
    private PaymentStatus held;
    private PaymentStatus stop;
    private PaymentStatus reisreq;
    private PaymentStatus reis;
    private PaymentStatus reisden;
    private PaymentStatus reml;
    private PaymentStatus lost;
    private PaymentStatus can;
    private PaymentStatus candum;
    private PaymentStatus outlaw;
    private PaymentMethodType warrant;
    private PaymentMethodType eft;
    private PaymentMethodType interfundVoucher;
    private PaymentMethodType recapture;
    private PaymentMethodType recovery;
    private PaymentMethodType journalVoucher;
    private PaymentMethodType offSystem;
    private PaymentMethodType zzzlocalImpressedFund;
    private DisbursementTranRlnRsn isRelatedTo;
    private CsePersonRlnRsn designatedPayee;
    private PaymentRequest refund;
    private PaymentRequest support;
    private PaymentRequest advancement;
  }
#endregion
}

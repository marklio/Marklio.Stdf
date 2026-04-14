using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SDR — Site Description Record (1, 80).
/// Contains configuration information for a site group (handler/prober setup). One record per site group.
/// </summary>
[StdfRecord(1, 80)]
public partial record class Sdr : IHeadRecord
{
    /// <summary>
    /// Test head number.
    /// [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Site group number.
    /// [STDF: SITE_GRP, U*1]
    /// </summary>
    public byte SiteGroup { get; set; }

    [WireCount("sites")] private byte SiteCount => throw new NotSupportedException();

    /// <summary>
    /// Array of site numbers in this group.
    /// [STDF: SITE_NUM, kxU*1]
    /// </summary>
    /// <remarks>
    /// Counted by SITE_CNT on the wire.
    /// </remarks>
    [CountedArray("sites")] public byte[]? SiteNumbers { get; set; }

    /// <summary>
    /// Handler or prober type.
    /// [STDF: HAND_TYP, C*n]
    /// </summary>
    public string? HandlerType { get; set; }

    /// <summary>
    /// Handler or prober ID.
    /// [STDF: HAND_ID, C*n]
    /// </summary>
    public string? HandlerId { get; set; }

    /// <summary>
    /// Interface card type.
    /// [STDF: CARD_TYP, C*n]
    /// </summary>
    public string? CardType { get; set; }

    /// <summary>
    /// Interface card ID.
    /// [STDF: CARD_ID, C*n]
    /// </summary>
    public string? CardId { get; set; }

    /// <summary>
    /// Load board type.
    /// [STDF: LOAD_TYP, C*n]
    /// </summary>
    public string? LoadboardType { get; set; }

    /// <summary>
    /// Load board ID.
    /// [STDF: LOAD_ID, C*n]
    /// </summary>
    public string? LoadboardId { get; set; }

    /// <summary>
    /// Device interface board type.
    /// [STDF: DIB_TYP, C*n]
    /// </summary>
    public string? DibType { get; set; }

    /// <summary>
    /// Device interface board ID.
    /// [STDF: DIB_ID, C*n]
    /// </summary>
    public string? DibId { get; set; }

    /// <summary>
    /// Interface cable type.
    /// [STDF: CABL_TYP, C*n]
    /// </summary>
    public string? CableType { get; set; }

    /// <summary>
    /// Interface cable ID.
    /// [STDF: CABL_ID, C*n]
    /// </summary>
    public string? CableId { get; set; }

    /// <summary>
    /// Contactor type.
    /// [STDF: CONT_TYP, C*n]
    /// </summary>
    public string? ContactorType { get; set; }

    /// <summary>
    /// Contactor ID.
    /// [STDF: CONT_ID, C*n]
    /// </summary>
    public string? ContactorId { get; set; }

    /// <summary>
    /// Laser type.
    /// [STDF: LASR_TYP, C*n]
    /// </summary>
    public string? LaserType { get; set; }

    /// <summary>
    /// Laser ID.
    /// [STDF: LASR_ID, C*n]
    /// </summary>
    public string? LaserId { get; set; }

    /// <summary>
    /// Extra equipment type.
    /// [STDF: EXTR_TYP, C*n]
    /// </summary>
    public string? ExtraType { get; set; }

    /// <summary>
    /// Extra equipment ID.
    /// [STDF: EXTR_ID, C*n]
    /// </summary>
    public string? ExtraId { get; set; }
}

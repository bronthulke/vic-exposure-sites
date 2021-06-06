using System.Collections.Generic;
using AWD.VicExposureSites;

public static class ManualDataUpdates {

    public static List<GeocodeDataItem> GetData()
    {
        return new List<GeocodeDataItem>() {
            new GeocodeDataItem() {
                address = "105 High Street, Axedale, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -36.7768707,
                    lng = 144.5040073
                },
            },
        };
    }
}
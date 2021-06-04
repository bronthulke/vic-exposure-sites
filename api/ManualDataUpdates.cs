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
            new GeocodeDataItem() {
                address = "Central Square Shopping Centre/Merton Street to Footscray Station, PTV Bus Number 412, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.820142,
                    lng = 144.849013
                },
            },
            new GeocodeDataItem() {
                address = "Central Square Shopping Centre/Merton Stop to Footscray Station, Bus Route, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.824927,
                    lng = 144.842367
                },
            },
            new GeocodeDataItem() {
                address = "Central Square Shopping Centre/Merton Street to Footscray Station, Bus Route, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.8712384,
                    lng = 144.7755724
                },
            },
            new GeocodeDataItem() {
                address = "Paisley Street/Footscray to Millers Road/Geelong, Bus Route, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.898631,
                    lng = 144.702345
                },
            },
            new GeocodeDataItem() {
                address = "Footscray Station, Paisley Street to Central Square Shopping Centre stop, Bus Route, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.817468,
                    lng = 144.846775
                },
            },
            new GeocodeDataItem() {
                address = "Central Square Shopping Centre/Merton Street to Footscray Station, Public Transport, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.8712384,
                    lng = 144.7755724
                },
            },
            new GeocodeDataItem() {
                address = "Central Square Shopping Centre/Merton Stop to Footscray Station, Public Transport, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.8712384,
                    lng = 144.7755724
                },
            },
            new GeocodeDataItem() {
                address = "Footscray Station, Paisley Street to Central Square Shopping Centre stop, Public Transport, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.801605,
                    lng = 144.9016139
                },
            },
            new GeocodeDataItem() {
                address = "Paisley Street/Footscray to Millers Road/Geelong Road, Public Transport, VIC, AU",
                location = new GeocodeDataLocationItem() {
                    lat = -37.8005314,
                    lng = 144.8963553
                },
            }       
        };
    }
}
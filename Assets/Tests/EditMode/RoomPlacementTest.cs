using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using dungeonGenerator;


public class RoomPlacementTest
{
    # region RoomPlacementInitialize
    [Test]
    [TestCaseSource(nameof(RoomPlacementInitializeCases))]
    public void RoomPlacementInitializeTest((Vector3Int minRoomBounds, Vector3Int totalRoomOffset, float corridorWidth) testData)
    {
        // check if initialize correctly

        Vector3Int minRoomBound = testData.minRoomBounds;
        Vector3Int totalRoomOffset = testData.totalRoomOffset;
        float corridorWidth = testData.corridorWidth;

        RoomCalculator roomCalculator = new RoomCalculator(minRoomBound, totalRoomOffset, corridorWidth);

        Assert.IsTrue(roomCalculator.minRoomBounds.Equals(minRoomBound), "minRoomBounds equal");
        Assert.IsTrue(roomCalculator.totalRoomOffset.Equals(totalRoomOffset), "totalRoomOffset equal");
        Assert.IsTrue(roomCalculator.corridorWidth.Equals(corridorWidth), "corridorWidth equal");

    }

    public static IEnumerable<(Vector3Int, Vector3Int, float)> RoomPlacementInitializeCases()
    {
        yield return (new Vector3Int(10, 10, 20), new Vector3Int(1, 1, 2), 1f);
        yield return (new Vector3Int(10, 5, 20), new Vector3Int(4, 3, 2), 1f);
        yield return (new Vector3Int(10, 10, 20), new Vector3Int(1, 1, 2), 1f);
    }

    #endregion

    #region PlaceRoomInSpaces
    [Test]
    [TestCaseSource(nameof(PlaceRoomsInSpacesCases))]
    public void PlaceRoomsInSpacesTest((List<Node> spaces, RoomCalculator roomCalculator) testData)
    {
        var roomCalculator = testData.roomCalculator;
        var spaces = testData.spaces;

        var placedRooms = roomCalculator.PlaceRoomsInSpaces(spaces);

        foreach (var room in placedRooms)
        {
            Assert.GreaterOrEqual(room.Bounds.size.x, roomCalculator.minRoomBounds.x);
            Assert.GreaterOrEqual(room.Bounds.size.z, roomCalculator.minRoomBounds.y);
        }
    }

    public static IEnumerable<(List<Node>, RoomCalculator)> PlaceRoomsInSpacesCases()
    {
        yield return ( GenerateRoomList(), 
            new RoomCalculator(
                new Vector3Int(10,10,10), 
                new Vector3Int(1,1,1), 
                1
            ));

        yield return (GenerateRoomList(),
         new RoomCalculator(
             new Vector3Int(3, 10, 4),
             new Vector3Int(1, 1, 4),
             1
         ));

        yield return (GenerateRoomList(),
         new RoomCalculator(
             new Vector3Int(10, 3, 4),
             new Vector3Int(3, 1, 4),
             1
         ));
    }

    public static List<Node> GenerateRoomList()
    {

        List<Node> roomList = new List<Node>();
        roomList.Add((Node)(new SpaceNode(
                new BoundsInt(
                    Vector3Int.zero,
                    new Vector3Int(40, 0, 40)
                ), null, 0, 0)));

        return roomList;

    }
    #endregion

}


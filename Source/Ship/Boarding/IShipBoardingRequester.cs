namespace InvadersOverboard.Ship.Boarding
{
    public interface IShipBoardingRequester
    {
        bool CanRequestBoarding(
            ShipBoardingPoint point);

        bool RequestBoarding(
            ShipBoardingPoint point);
    }
}

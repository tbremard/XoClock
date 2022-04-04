namespace XoClock
{
    class MotionGenerator
    {
        private const int MOVE_FAST_TRIGGER = 5;
        private const int MOVE_ULTRA_FAST_TRIGGER = 20;
        private const int MOVE_ULTRA_FAST_SPEED = 50;
        private const int MOVE_FAST_SPEED = 10;
        private const int MOVE_SLOW_SPEED = 1;
        int _counter = 0;


        public double Continue()
        {
            double ret;
            _counter++;
            if (_counter >  MOVE_ULTRA_FAST_TRIGGER)
            {
                ret = MOVE_ULTRA_FAST_SPEED;
            }
            else if (_counter > MOVE_FAST_TRIGGER)
            {
                ret = MOVE_FAST_SPEED;
            }
            else
            {
                ret = MOVE_SLOW_SPEED;
            }
            return ret;
        }

        public void Stop()
        {
            _counter = 0;
        }

    }
}

using System;
using System.Threading;
using static MonitorActividad.App.NativeMethods;

namespace MonitorActividad
{
    internal class SystemInputsRefresh
    {
        static public Timer FocusInputsIntervals;

        static uint JOY_RETURNALL = 0x00000001 | 0x00000002 | 0x00000004 | 0x00000008 | 0x00000010 | 0x00000020 | 0x00000040 | 0x00000080;

        static LPJOYINFOEX JoystickInfo = new LPJOYINFOEX() { dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(LPJOYINFOEX)), dwFlags = JOY_RETURNALL };
        static LPJOYCAPS JoystickCapInfo = new LPJOYCAPS();

        static long lastKeyboardChange;
        static long lastMouseChange;
        static long lastJoystickChange;

        static uint[] lastJoystickPOVlist = new uint[16];
        public static int CustomJoyDeadZone;

        static POINT lastCursorPos = new POINT();
        static POINT currentCursorPos = new POINT();

        static long currentTick;
        static bool onShedule = false;

        static public bool lastState = false;
        static public long lastActiveTime = App.GetTimeSinceSysStart();
        static public long activeTime = 0;
        static public long initialActiveTime = App.GetTimeSinceSysStart();

        static public long estateActiveTime = App.GetTimeSinceSysStart();

        static bool CheckAllKeyboardInputs()
        {
            for (int i = 7; i < 256; i++)
            {
                if (GetKeyState(i) < 0)
                {
                    //Console.WriteLine("keyboard: " + i + " " + Environment.TickCount);
                    lastKeyboardChange = currentTick;
                    return true;
                }
            }
            if (GetKeyState(3) < 0)
            {
                //Console.WriteLine("keyboard: 3 " + Environment.TickCount);
                lastKeyboardChange = currentTick;
                return true;
            }

            return false;
        }

        static bool CheckAllMouseInputs()
        {
            GetCursorPos(ref currentCursorPos);

            if (currentCursorPos.x != lastCursorPos.x || currentCursorPos.y != lastCursorPos.y)
            {
                //Console.WriteLine("change: " + currentCursorPos.x + " " + currentCursorPos.y + " " + Environment.TickCount);
                lastCursorPos.x = currentCursorPos.x;
                lastCursorPos.y = currentCursorPos.y;
                lastMouseChange = currentTick;
                return true;
            }
            if (GetKeyState(1) < 0 || GetKeyState(2) < 0 || GetKeyState(4) < 0 || GetKeyState(5) < 0 || GetKeyState(6) < 0)
            {
                //Console.WriteLine("mouse:" + Environment.TickCount);
                lastMouseChange = currentTick;
                return true;
            }

            return false;
        }

        static bool CheckAllJoysticksInputs()
        {
            for (uint i = 0; i < joyGetNumDevs(); ++i)
            {
                if (joyGetPosEx(i, ref JoystickInfo) == 0)
                {
                    if (JoystickInfo.dwButtonNumber != 0)
                    {
                        //Console.WriteLine("Pushing button: " + i + " " + Environment.TickCount);
                        lastJoystickChange = currentTick;
                        return true;
                    }

                    int JoystickThreshold = 0;
                    joyGetThreshold(i, ref JoystickThreshold);
                    joyGetDevCaps(i, ref JoystickCapInfo, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(LPJOYCAPS)));

                    if ((Math.Abs((int)JoystickInfo.dwXpos-(JoystickCapInfo.wXmin+JoystickCapInfo.wXmax)/2) > JoystickThreshold+CustomJoyDeadZone) ||
                    (Math.Abs((int)JoystickInfo.dwYpos-(JoystickCapInfo.wYmin+JoystickCapInfo.wYmax)/2) > JoystickThreshold+CustomJoyDeadZone) ||
                    ((JoystickCapInfo.wCaps & 1) != 0 && Math.Abs((int)JoystickInfo.dwZpos-(JoystickCapInfo.wZmin+JoystickCapInfo.wZmax)/2) > JoystickThreshold+CustomJoyDeadZone) ||
                    ((JoystickCapInfo.wCaps & 2) != 0 && Math.Abs((int)JoystickInfo.dwRpos-(JoystickCapInfo.wRmin+JoystickCapInfo.wRmax)/2) > JoystickThreshold+CustomJoyDeadZone) ||
                    ((JoystickCapInfo.wCaps & 4) != 0 && Math.Abs((int)JoystickInfo.dwUpos-(JoystickCapInfo.wUmin+JoystickCapInfo.wUmax)/2) > JoystickThreshold+CustomJoyDeadZone) ||
                    ((JoystickCapInfo.wCaps & 8) != 0 && Math.Abs((int)JoystickInfo.dwVpos-(JoystickCapInfo.wVmin+JoystickCapInfo.wVmax)/2) > JoystickThreshold+CustomJoyDeadZone))
                    {
                        //Console.WriteLine("Moving Axis: " + i + " " + Environment.TickCount);
                        lastJoystickChange = currentTick;
                        return true;
                    }

                    if ((JoystickCapInfo.wCaps & 16) != 0)
                    {
                        if ((JoystickCapInfo.wCaps & 64) != 0)
                        {
                            if (lastJoystickPOVlist[(int)i] != JoystickInfo.dwPOV)
                            {
                                //Console.WriteLine("POV Continuous: " + i + " " + Environment.TickCount);
                                lastJoystickChange = currentTick;
                                lastJoystickPOVlist[(int)i] = JoystickInfo.dwPOV;
                                return true;
                            }
                        }
                        else if (JoystickInfo.dwPOV != 65535)
                        {
                            //Console.WriteLine("POV Directional: " + i + " " + Environment.TickCount);
                            lastJoystickChange = currentTick;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static public void RefreshAllFocusInputs(object stateInfo)
        {
            currentTick = App.GetTimeSinceSysStart();

            bool NewMouseInput = CheckAllMouseInputs();
            bool NewKeyboardInput = CheckAllKeyboardInputs();
            bool NewJoystickInput = CheckAllJoysticksInputs();

            if (onShedule) 
            {
                if (DateTime.Now.TimeOfDay >= App.horarioFin || DateTime.Now.TimeOfDay < App.horarioIni)
                {
                    onShedule = false;
                    App.ActualizarActividad(App.UsuarioActividadHoy);
                    //Console.WriteLine("EEEEEOO2");
                }
            }
            else
            {
                if (DateTime.Now.TimeOfDay < App.horarioFin || DateTime.Now.TimeOfDay >= App.horarioIni)
                {
                    onShedule = true;
                    App.RestaurarSoloEstadoLocal();
                    //Console.WriteLine("EEEEEOOOO");
                }
            }

            if (App.UsuarioSesion != -1 && App.enPerfil && onShedule)
            {
                if (lastState)
                {
                    if (!(NewJoystickInput || NewKeyboardInput || NewMouseInput))
                    {
                        //Console.WriteLine("***Deactive Inputs Key*** " + " " + Environment.TickCount);
                        activeTime += currentTick - initialActiveTime;
                        lastActiveTime = currentTick;
                        lastState = false;
                    }
                }
                else
                {
                    if (NewJoystickInput || NewKeyboardInput || NewMouseInput)
                    {
                        //Console.WriteLine("***Active Inputs Key*** " + " " + Environment.TickCount);
                        lastActiveTime = currentTick;
                        lastState = true;

                        if (App.categoria == 0 && App.estado == 1)
                        {
                            App.neutral += currentTick - estateActiveTime;
                        }
                        if (App.estado == 2)
                        {
                            App.inactivo += currentTick - estateActiveTime;
                        }
                        if (App.estado == 0)
                        {
                            if (App.categoria == 0)
                            {
                                App.productivo += currentTick - initialActiveTime;
                            }
                            if (App.categoria == 1)
                            {
                                App.neutral += currentTick - initialActiveTime;
                            }
                            if (App.categoria == 2)
                            {
                                App.noProductivo += currentTick - initialActiveTime;
                            }
                            if (App.categoria == 3)
                            {
                                App.noCategorizado += currentTick - initialActiveTime;
                            }
                        }

                        initialActiveTime = currentTick;

                        App.estado = 0;
                    }

                    if (App.estado == 0 && (((double)(currentTick - lastActiveTime) / 1000) > App.tiempoNeutral))
                    {
                        if (App.categoria == 0)
                        {
                            App.productivo += currentTick - initialActiveTime;
                            initialActiveTime = currentTick;
                            App.estado = 1;
                            estateActiveTime = currentTick;
                        }
                    }
                    if (App.estado < 2 && (((double)(currentTick - lastActiveTime) / 1000) > App.tiempoInactivo))
                    {
                        if (App.categoria == 0)
                        {
                            App.neutral += currentTick - initialActiveTime;
                        }
                        if (App.categoria == 1)
                        {
                            App.neutral += currentTick - initialActiveTime;
                        }
                        if (App.categoria == 2)
                        {
                            App.noProductivo += currentTick - initialActiveTime;
                        }
                        if (App.categoria == 3)
                        {
                            App.noCategorizado += currentTick - initialActiveTime;
                        }

                        App.estado = 2;
                        estateActiveTime = currentTick;
                    }
                }
            }

            //Console.WriteLine((double)(activeTime)/ 1000);

            /*
            Console.WriteLine("PRODUCTIVO: " + (double)(App.productivo) / 1000);
            Console.WriteLine("NEUTRAL: " + (double)(App.neutral) / 1000);
            Console.WriteLine("NO PRODUCTIVO: " + (double)(App.noProductivo) / 1000);
            Console.WriteLine("NO CATEGORIZADO: " + (double)(App.noCategorizado) / 1000);
            Console.WriteLine("INACTIVO: " + (double)(App.inactivo) / 1000);
            Console.WriteLine("ESTADO: "+App.estado);
            */

            //Console.WriteLine((double)(activeTime + (currentTick - lastActiveTime)) / 1000);

            FocusInputsIntervals.Change(20, Timeout.Infinite);
        }
    }
}

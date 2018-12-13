using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace advanced_delegates
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Creator: Felipe Bossolani - fbossolani[at]gmail.com");
            Console.WriteLine(@"Examples based on: http://returnsmart.blogspot.com/2015/07/mcsd-programming-in-c-part-3-70-483.html");
            Console.WriteLine("Choose a Delegate Method: ");
            Console.WriteLine("01 - Basic Delegate");
            Console.WriteLine("02 - Simple Multicast Delegate");
            Console.WriteLine("03 - Simple Delegate Lambda Expression");
            Console.WriteLine("04 - Func and Action Delegate");
            Console.WriteLine("05 - Closure Delegate");
            Console.WriteLine("06 - Simple Event/Callback");
            Console.WriteLine("07 - Advanced Event/Callback");
            Console.WriteLine("08 - Advanced Event/Callback - using Add/Remove and Lock");
            Console.WriteLine("09 - Exceptions Event/Callback");

            int.TryParse(Console.ReadLine(), out var option);
            
            switch (option)
            {
                case 1:
                    {
                        BasicDelegate();
                        break;
                    }
                case 2:
                    {
                        SimpleMulticastDelegate();
                        break;
                    }

                case 3:
                    {
                        SimpleLambdaDelegate();
                        break;
                    }
                case 4:
                    {
                        FuncActionDelegate();
                        break;
                    }
                case 5:
                    {
                        ClosureDelegate();
                        break;
                    }
                case 6:
                    {
                        SimpleEventCallback();
                        break;
                    }
                case 7:
                    {
                        AdvancedEventCallback();
                        break;
                    }

                case 8:
                    {
                        AdvancedEventCallback2();
                        break;
                    }
                case 9:
                    {
                        ExceptionEventCallback();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Invalid option...");
                        break;
                    }
            }
        }

        public delegate int Calculate(int x, int y);

        public static int Add(int x, int y) {
            Console.WriteLine("Add({0},{1}) = {2}", x, y, x + y);
            return x + y;
        }
        public static int Multiply(int x, int y) {
            Console.WriteLine("Multiply({0},{1}) = {2}", x, y, x * y);
            return x * y;
        }

        public static void BasicDelegate()
        {
            int x = 3;
            int y = 4;
            Calculate calc = Add;
            Console.WriteLine(calc(x,y));

            calc = Multiply;
            Console.WriteLine(calc(x, y));

        }

        public static void SimpleMulticastDelegate()
        {
            Console.WriteLine("Adding Add(x, y)");
            Calculate calc = Add;
            calc(4, 3);

            Console.WriteLine("Adding Multiply(x, y)");
            calc += Multiply;
            calc(4,3);

            Console.WriteLine("Removing Add(x, y)");

            calc -= Add;
            calc(4, 3);
        }

        public static void FuncActionDelegate()
        {
            Func<int, int, int> calc =
                   (x, y) =>
                   {
                       Console.WriteLine("Using Func<int,int,int> thats return an int value");
                       return x + y;
                   };

            Console.WriteLine(calc(5,3));

            Action<int, int> calc2 =
                (x, y) => {
                    Console.WriteLine("Using Action<int,int> thats doesnt return any value" );
                    Console.WriteLine(x + y);
                };

            calc2(5, 3);
        }

        public static void SimpleLambdaDelegate()
        {
            Calculate calc =
                (x, y) =>
                {
                    Console.WriteLine("I was created by a lambda expression");
                    return x + y;
                };

            Console.WriteLine(calc(5,3));
        }

        /*
        Practically, a closure is a block of code which can be executed at a later time, 
        but which maintains the environment in which it was first created. 
        It can use local variables of the method where was created, even if the method is finished. 
        It's implemented by anonymous methods (lambda exp) like here:
        */
        public static void ClosureDelegate()
        {
            Action del = GetClosureDelegate();
            del();
            del();
            del();
        }

        public static Action GetClosureDelegate()
        {
            int counter = 0;
            return delegate
            {
                Console.WriteLine("Counter={0}", counter);
                counter++;
            };
        }

        public static void SimpleEventCallback()
        {
            EventCallback1 e = new EventCallback1();
            e.OnChange += () => { Console.WriteLine("First time"); };
            e.OnChange += () => { Console.WriteLine("Second time"); };
            e.Raise();
        }

        public static void AdvancedEventCallback()
        {
            EventSubscriber sub = new EventSubscriber();
            sub.Run();
        }

        public static void AdvancedEventCallback2()
        {
            EventSubscriber2 sub = new EventSubscriber2();
            sub.Run();
        }

        public static void ExceptionEventCallback()
        {
            EventSubscriber3 sub = new EventSubscriber3();
            sub.Run();
        }

        public class EventCallback1
        {
            public Action OnChange { get; set; }

            public void Raise()
            {
                Console.WriteLine("Calling Raise() method");
                if (OnChange != null)
                {
                    OnChange();
                }
            }
        }

        public class MyArgs : EventArgs
        {
            public int value { get; set; }

            public MyArgs(int i)
            {
                value = i;
            }
        }

        public class EventPublisher
        {
            public event EventHandler<MyArgs> OnChange = delegate {};

            public void Raise()
            {
                OnChange(this, new MyArgs(20));
            }
        }

        public class EventPublisher2
        {
            public event EventHandler<MyArgs> onChange = delegate { };
            public event EventHandler<MyArgs> OnChange
            {
                add
                {
                    lock (onChange)
                    {
                        onChange += value;
                    }
                }
                remove
                {
                    lock (onChange)
                    {
                        onChange -= value;
                    }
                }
            }


            public void Raise()
            {
                onChange(this, new MyArgs(40));
            }

        }

        public class EventPublisher3
        {
            public event EventHandler<MyArgs> OnChange = delegate { };

            public void Raise()
            {
                var exceptions = new List<Exception>();
                int i = 1;
                foreach (Delegate handler in OnChange.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke(this, new MyArgs(i++));
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                        Console.WriteLine("Error here {0}", e);
                    }
                }
            }
        }

        public class EventSubscriber
        {
            public void Run()
            {
                EventPublisher pub = new EventPublisher();

                pub.OnChange += (sender, e) => { Console.WriteLine("OnChange: Event raised {0}", e.value); };
                pub.Raise();
            }
        }

        public class EventSubscriber2
        {
            public void Run()
            {
                EventPublisher2 pub = new EventPublisher2();

                pub.OnChange += (sender, e) => { Console.WriteLine("OnChange: Event raised {0}", e.value); };
                pub.Raise();
            }
        }

        public class EventSubscriber3
        {
            public void Run()
            {
                EventPublisher3 pub = new EventPublisher3();

                pub.OnChange += (sender, e) => { Console.WriteLine("OnChange: Event raised {0}", e.value); };
                pub.OnChange += (sender, e) => { throw new Exception(); };
                pub.OnChange += (sender, e) => { Console.WriteLine("OnChange: Event raised {0}", e.value); };
                pub.Raise();
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace HospitalAppointmentSystem
{
    public class User
    {
        public string Name { get; }
        public string Surname { get; }
        public string Email { get; }
        public string Phone { get; }

        public User(string name, string surname, string email, string phone)
        {
            Name = name;
            Surname = surname;
            Email = email;
            Phone = phone;
        }
    }

    public class Appointment
    {
        public string DoctorName { get; set; }
        public string DoctorSurname { get; set; }
        public string Time { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
    }

    public class AppointmentSlot
    {
        public string Time { get; }
        public AppointmentSlot(string time)
        {
            Time = time;
        }
    }

    public class Doctor
    {
        public string Name { get; }
        public string Surname { get; }
        public int Experience { get; }
        public List<AppointmentSlot> Slots { get; }

        public Doctor(string name, string surname, int experience)
        {
            Name = name;
            Surname = surname;
            Experience = experience;
            Slots = new List<AppointmentSlot>
            {
                new("09:00-11:00"),
                new("12:00-14:00"),
                new("15:00-17:00")
            };
        }
    }

    public class Department
    {
        public string Name { get; }
        public List<Doctor> Doctors { get; }

        public Department(string name, List<Doctor> doctors)
        {
            Name = name;
            Doctors = doctors;
        }
    }
}



namespace HospitalAppointmentSystem
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            AppointmentSystem.Run();
        }
    }
}



namespace HospitalAppointmentSystem
{
    public static class AppointmentSystem
    {
        private static readonly List<Department> Departments = new();
        private static List<Appointment> Appointments = new();
        private const string JsonFile = "appointments.json";

        public static void Run()
        {
            SeedData();
            LoadAppointments();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Hospital System\n");

                Department dept = SelectDepartment();
                Doctor doctor = SelectDoctor(dept);
                DateTime date = GetDate();
                User user = GetUserInfo();
                string time = SelectAvailableSlot(doctor, date);

                Appointment newAppt = new Appointment
                {
                    UserName = user.Name,
                    UserSurname = user.Surname,
                    DoctorName = doctor.Name,
                    DoctorSurname = doctor.Surname,
                    Time = time,
                    Date = date

                };

                Appointments.Add(newAppt);
                SaveAppointments();

                Console.WriteLine($"\n✅ Təşəkkürlər {user.Name} {user.Surname}, siz {date:yyyy-MM-dd} tarixində saat {time} üçün {doctor.Name} {doctor.Surname} həkimin qəbuluna yazıldınız.");
                Console.WriteLine("\nYeni istifadəçi üçün Enter basın...");
                Console.ReadLine();
            }
        }

        private static void SeedData()
        {
            if (Departments.Count > 0) return;

            Departments.Add(new Department("Pediatriya", new List<Doctor>
            {
                new("Aydub", "Ekberov", 3),
                new("Veli", "Veliyev", 5),
                new("Fuad", "İskenderli", 2)
            }));

            Departments.Add(new Department("Travmatologiya", new List<Doctor>
            {
                new("Oruc", "Burhanov", 10),
                new("İbad", "Aliyev", 7)
            }));

            Departments.Add(new Department("Stomatologiya", new List<Doctor>
            {
                new("Temkin", "İsmayılov", 4),
                new("İdrak", "İskenderli", 6),
                new("Aysu", "Səfərova", 9),
                new("Fatime", "Uğurova", 2)
            }));
        }

        private static void LoadAppointments()
        {
            if (File.Exists(JsonFile))
            {
                string json = File.ReadAllText(JsonFile);
                Appointments = JsonSerializer.Deserialize<List<Appointment>>(json) ?? new();
            }
        }

        private static void SaveAppointments()
        {
            string json = JsonSerializer.Serialize(Appointments, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonFile, json);
        }

        private static User GetUserInfo()
        {
            Console.Write("Ad: ");
            string name = Console.ReadLine();
            Console.Write("Soyad: ");
            string surname = Console.ReadLine();
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Telefon: ");
            string phone = Console.ReadLine();

            return new User(name, surname, email, phone);
        }

        private static DateTime GetDate()
        {
            while (true)
            {
                Console.Write("\n Qəbul üçün tarix seçin (YYYY-MM-DD): ");
                string input = Console.ReadLine();

                if (DateTime.TryParse(input, out DateTime date) && date >= DateTime.Today)
                    return date;

                Console.WriteLine("❗ Tarix düzgün deyil və ya keçmişdədir. Yenidən cəhd edin.");
            }
        }

        private static Department SelectDepartment()
        {
            Console.WriteLine("\n🔹 Şöbələr:");
            for (int i = 0; i < Departments.Count; i++)
                Console.WriteLine($"{i + 1}. {Departments[i].Name}");

            int choice = GetChoice("Şöbə seçin: ", 1, Departments.Count);
            return Departments[choice - 1];
        }

        private static Doctor SelectDoctor(Department dept)
        {
            Console.WriteLine($"\n🔸 {dept.Name} şöbəsində mövcud həkimlər:");
            for (int i = 0; i < dept.Doctors.Count; i++)
                Console.WriteLine($"{i + 1}. {dept.Doctors[i].Name} {dept.Doctors[i].Surname} ({dept.Doctors[i].Experience} il)");

            int choice = GetChoice("Həkim seçin: ", 1, dept.Doctors.Count);
            return dept.Doctors[choice - 1];
        }

        private static string SelectAvailableSlot(Doctor doctor, DateTime date)
        {
            while (true)
            {
                Console.WriteLine($"\n {doctor.Name} {doctor.Surname} üçün {date:yyyy-MM-dd} tarixində saatlar:");
                for (int i = 0; i < doctor.Slots.Count; i++)
                {
                    string time = doctor.Slots[i].Time;
                    bool isReserved = Appointments.Any(a =>
                        a.DoctorName == doctor.Name &&
                        a.DoctorSurname == doctor.Surname &&
                        a.Date.Date == date.Date &&
                        a.Time == time);

                    string status = isReserved ? "Doludur" : "Bosdur";
                    Console.WriteLine($"{i + 1}. {time} - {status}");
                }

                int choice = GetChoice("Saat teryin edin: ", 1, doctor.Slots.Count);
                string selectedTime = doctor.Slots[choice - 1].Time;

                bool reserved = Appointments.Any(a =>
                    a.DoctorName == doctor.Name &&
                    a.DoctorSurname == doctor.Surname &&
                    a.Date.Date == date.Date &&
                    a.Time == selectedTime);

                if (reserved)
                {
                    Console.WriteLine(" Bu vaxt artıq rezervasiya edilibç başqa bir vaxtı rezervasiya edin .");
                }
                else
                {
                    return selectedTime;
                }
            }
        }

        private static int GetChoice(string message, int min, int max)
        {
            int choice;
            do
            {
                Console.Write(message);
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < min || choice > max);

            return choice;
        }
    }
}

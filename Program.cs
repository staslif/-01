using System;
using System.Threading;

class Account
{
    private decimal balance;
    private readonly object balanceLock = new object();

    public Account(decimal initialBalance)
    {
        balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        lock (balanceLock)
        {
            balance += amount;
            Console.WriteLine($"Пополнение: {amount}. Текущий баланс: {balance}");
            Monitor.PulseAll(balanceLock); // Уведомляем ожидающие потоки
        }
    }

    public void Withdraw(decimal amount)
    {
        lock (balanceLock)
        {
            while (balance < amount)
            {
                Console.WriteLine($"Ожидание пополнения для снятия: {amount}. Текущий баланс: {balance}");
                Monitor.Wait(balanceLock); // Ожидаем, пока баланс не станет достаточным
            }
            balance -= amount;
            Console.WriteLine($"Снятие: {amount}. Остаток на счете: {balance}");
        }
    }

    public decimal GetBalance()
    {
        lock (balanceLock)
        {
            return balance;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Account account = new Account(0);
        Random random = new Random();

        // Поток для пополнения счета
        Thread depositThread = new Thread(() =>
        {
            while (true)
            {
                decimal amount = random.Next(10, 100); // 
                account.Deposit(amount);
                Thread.Sleep(random.Next(500, 2000)); // Задержка между пополнениями
            }
        });

        depositThread.Start();

        // Снятие денег
        Thread withdrawThread = new Thread(() =>
        {
            while (true)
            {
                decimal amountToWithdraw = random.Next(50, 150); // Сумма для снятия
                account.Withdraw(amountToWithdraw);
                Thread.Sleep(random.Next(1000, 3000)); // Задержка между снятиями
            }
        });

        withdrawThread.Start();

        // Вывода остатка на балансе
        while (true)
        {
            Console.WriteLine($"Текущий баланс: {account.GetBalance()}");
            Thread.Sleep(2000); // Задержка для вывода баланса
        }
    }
}
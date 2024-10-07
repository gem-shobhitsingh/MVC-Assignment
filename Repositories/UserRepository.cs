using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using User_Management.Models;
using User_Management.ViewModels;

public class UserRepository
{
    // Dictionary to hold user data
    private Dictionary<string, UserViewModel> users = new Dictionary<string, UserViewModel>();

    public void RegisterUser(UserViewModel user)
    {
        if (!users.ContainsKey(user.Email))
        {
            users[user.Email] = user; // Store user details
        }
    }

    public UserViewModel AuthenticateUser(string email, string password)
    {
        // Validate user credentials
        if (users.ContainsKey(email) && users[email].Password == password)
        {
            return users[email]; // Return user details
        }
        return null; // Authentication failed
    }

    public void PrintUsers()
    {
        Debug.WriteLine("Users in the dictionary:");
        foreach (var user in users)
        {
            Debug.WriteLine($"Email: {user.Key}, Password: {user.Value}");
        }
    }
}

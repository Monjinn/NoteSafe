using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoteSafe.Classes
{
    public class Entry : IDisposable
    {
        private readonly long id;
        private readonly string name;
        private readonly string username;
        private readonly string password;
        private Category category;

        // IDisposable interface
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public long ID
        {
            get { return this.id; }
        }

        public String Name
        {
            get { return this.name; }
        }

        public String Username
        {
            get { return this.username; }
        }

        public String Password
        {
            get { return this.password; }
        }

        public Category Category 
        {
            get { return this.category; }
            set { this.category = value; }
        }

        /// <summary>
        /// Constructor for Entry
        /// </summary>
        /// <param name="name">Name for the entry, to identify the service or website.</param>
        /// <param name="username">Username for the entry.</param>
        /// <param name="pw">Password.</param>
        /// <param name="cat">Category for the entry.</param>
        public Entry(long id, String name, String username, String pw, Category cat)
        {
            this.id = id;
            this.name = name;
            this.username = username;
            this.password = pw;
            Category = cat;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            disposed = true;
        }
    }
}

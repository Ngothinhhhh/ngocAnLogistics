using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.Models;
using static WebApplication3.Models.Driver;
using static WebApplication3.Models.Order;

namespace WebApplication3.Datas
{
    public class FleetDB : DbContext
    {
        public DbSet<Customer> Customers { get; set; }   // EF Core sẽ dùng DbSet để map giữa class C# và bảng trong DB
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<User> User { get; set; }

        public DbSet<Image> Image { get; set; }    


        public FleetDB(DbContextOptions<FleetDB> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne(o => o.FromLocation)
                .WithMany(l => l.OrdersFromLocation)
                .HasForeignKey(o => o.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.FromWhere)
                .WithMany(l => l.OrdersFromWhere)
                .HasForeignKey(o => o.FromWhereId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.ToLocation)
                .WithMany(l => l.OrdersToLocation)
                .HasForeignKey(o => o.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với Truck
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Truck)
                .WithMany(t => t.OrdersAsTruck) // nếu Truck có collection này
                .HasForeignKey(o => o.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với Rmooc
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Rmooc)
                .WithMany(t => t.OrdersAsRmooc) // nếu Truck có collection này
                .HasForeignKey(o => o.RmoocId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Item)
                .WithMany(i => i.OrderLines)
                .HasForeignKey(ol => ol.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Images)
                .HasForeignKey(i => i.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Image>()
                .HasOne(u => u.User)
                .WithMany(o => o.Images)
                .HasForeignKey(i => i.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            //User-Activity : 1-n
            modelBuilder.Entity<Activity>()
                .HasOne(u => u.User)
                .WithMany( a => a.Activities)
                .HasForeignKey(i => i.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            //Order-Activity : 1-n
            modelBuilder.Entity<Activity>()
                .HasOne(u => u.Order)
                .WithMany( a => a.Activitys)
                .HasForeignKey(i => i.OrderID)
                .OnDelete(DeleteBehavior.Cascade);


            // Quan hệ 1-1: User - Customer
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1-1: User - Driver
            modelBuilder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithOne(u => u.Driver)
                .HasForeignKey<Driver>(d => d.UserID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>() // Lưu enum thành string thay vì int
            .HasColumnType("ENUM('Pending','InProgress','PickedUp','InTransit','Delivered','AwaitingApproval','Completed','Cancelled','FailedDelivery')")
            .HasDefaultValue(OrderStatus.Pending);
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Driver>()
            .Property(o => o.Status)
            .HasConversion<string>() // Lưu enum thành string thay vì int
            .HasColumnType("ENUM('Available','Busy')")
            .HasDefaultValue(StatusDriver.Available);


            modelBuilder.Entity<Order>()
                .Property(o => o.RowVersion)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();



        }



    }
}

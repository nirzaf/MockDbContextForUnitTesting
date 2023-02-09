using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Entities;

public class ApplicationDbContext : DbContext
{
 public ApplicationDbContext(DbContextOptions options) : base(options)
 {
 }

 public virtual DbSet<Country> Countries { get; set; }
 public virtual DbSet<Person> Persons { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  base.OnModelCreating(modelBuilder);

  modelBuilder.Entity<Country>().ToTable("Countries");
  modelBuilder.Entity<Person>().ToTable("Persons");

  //Seed to Countries
  string countriesJson = File.ReadAllText("countries.json");
  List<Country> countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);

  foreach (Country country in countries)
   modelBuilder.Entity<Country>().HasData(country);

  //Seed to Persons
  string personsJson = File.ReadAllText("persons.json");
  List<Person> persons = JsonSerializer.Deserialize<List<Person>>(personsJson);

  foreach (Person person in persons)
   modelBuilder.Entity<Person>().HasData(person);


  //Fluent API
  modelBuilder.Entity<Person>().Property(temp => temp.TIN)
   .HasColumnName("TaxIdentificationNumber")
   .HasColumnType("varchar(8)")
   .HasDefaultValue("ABC12345");

  //modelBuilder.Entity<Person>()
  //  .HasIndex(temp => temp.TIN).IsUnique();

  modelBuilder.Entity<Person>()
   .HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");

  //Table Relations
  modelBuilder.Entity<Person>(entity =>
  {
   entity.HasOne<Country>(c => c.Country)
    .WithMany(p => p.Persons)
    .HasForeignKey(p => p.CountryID);
  });
 }

 public List<Person> sp_GetAllPersons()
 {
  return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
 }

 public int sp_InsertPerson(Person person)
 {
  SqlParameter[] parameters = {
   new("@PersonID", person.PersonID),
   new("@PersonName", person.PersonName),
   new("@Email", person.Email),
   new("@DateOfBirth", person.DateOfBirth),
   new("@Gender", person.Gender),
   new("@CountryID", person.CountryID),
   new("@Address", person.Address),
   new("@ReceiveNewsLetters", person.ReceiveNewsLetters)
  };

  return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
 }
}
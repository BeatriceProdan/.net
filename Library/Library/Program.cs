using System;
using System.Collections.Generic;
using System.Linq;
//top-level statements 
List<Book> books = new List<Book>();

Console.WriteLine();
Console.WriteLine("Welcome to the Library Management System!");

//add books
while (true)
{
    Console.WriteLine();
    Console.Write("Enter a new book title: (or type 'exit' to finish) ");
    string? title = Console.ReadLine();

    if (string.Equals(title, "exit", StringComparison.InvariantCultureIgnoreCase))
        break;
    
    Console.Write("Enter the year published: ");
    int yearPublished = int.TryParse(Console.ReadLine(), out int y) ? y : 0;
    
    books.Add(new Book(title!, yearPublished));
}

//display all books
Console.WriteLine("\nAll books in the library:");
books.ForEach(b=> Console.WriteLine($"{b.Title}, published in {b.YearPublished}"));

//pattern matching
void DisplayInfo(object obj) => Console.WriteLine(obj switch
{
    Book b => $"Book: {b.Title} ({b.YearPublished})",
    Borrower br => $"Borrower: {br.Name}, Books borrowed: {br.BorrowedBooks.Count}",
    _ => "Unknown type"
});

//static lambda for filtering books
static bool PublishedAfter2010(Book b) => b.YearPublished > 2010;

var recentBooks = books.Where(PublishedAfter2010);
Console.WriteLine("\nBooks published after 2010:");
foreach (var book in recentBooks)
{
    Console.WriteLine($"{book.Title}, published in {book.YearPublished}");
}

//borrower cloning
var borrower1 = new Borrower("aa1", "Beatrice", new List<Book> { new Book("1984", 1949) });
var newBook = new Book("Oliver Twist", 1837);
var newBook2 = new Book("Recent Book", 2011);
var borrower2 = borrower1 with { BorrowedBooks = borrower1.BorrowedBooks.Append(newBook).ToList() };

Console.WriteLine($"\nBorrower clone demo:");
Console.WriteLine($"{borrower1.Name} initially borrowed {borrower1.BorrowedBooks.Count} book(s)");
Console.WriteLine($"{borrower2.Name} now borrowed {borrower2.BorrowedBooks.Count} book(s)");

//records
public record Book(string Title, int YearPublished);

public record Borrower(string Id, string Name, List<Book> BorrowedBooks);

//init-only properties user
public class Librarian(string name, string email, string librarySection)
{
    public string Name { get; init; } = name;
    public string Email { get; init; } = email;
    public string LibrarySection { get; init; } = librarySection;
}




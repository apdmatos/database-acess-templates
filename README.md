#database-acess-templates

database-access-templates is a simple and high performance object mapper for relational databases.

The goal is to load data from your relational database without writing much code.

## supported databases

database-access-templates supports every relational database since we are using abstract objects and interfaces from the namespace System.Data.Common, such as:
 - DbConnection
 - DbCommand
 - IDataReader
 - ...


 ## how to use it

 In your repositories to load data from your database, you can write:


	 	public class Car 
	 	{
	 		public string Id { get; set; };

	 		public string Brand { get; set; };

	 		public int Year { get; set; };

	 		public string Color { get; set; };
	 	}

	 	public class CarRepository
	    {

	        public CarRepository() 
	        { 
	        	Connection = new SqlConnection(...);
	        }
	        public CarRepository(DbConnection connection) 
	        {
	            Connection = connection;
	        }


	        public Car GetCarById(string carId)
	        {
	            return DbTemplate<Car>.GetObjectBySQLQuery(
	                    Connection,
	                    DataReader2Car,
	                    string.Format(@"select id, brand, year, color 
	                    				from  cars
	                    				where indicator_id=@carId"),
	                    new DbTemplateParameter[]
	                    {
	                        new DbTemplateParameter(DbType.String, "carId", carId)
	                    });
	        }

	        public IEnumerable<Car> GetCars(int? page, int? recordsPerPage)
	        {
	            return DbTemplate<Car>.GetListByProcedure(
	                    Connection,
	                    DataReader2Car,
	                    "getCars",
	                    new DbTemplateParameter[]
	                    {
	                        new DbTemplateParameter(DbType.Int32, "p_page", ReturnsDefaultDbNumber(page)),
	                        new DbTemplateParameter(DbType.Int32, "p_recordsPerPage", ReturnsDefaultDbNumber(recordsPerPage))
	                    });
	        }

	        private object ReturnsDefaultDbNumber(int? n)
	        {
	            if (n.HasValue) return n.Value;
	            return DBNull.Value;
	        }

	        public Car DataReader2Car(IDataReader reader)
	        {
	            if (!reader.HasColumn("id")) return null;
	            return new Car
	            {
	                Id          = (string)reader["id"],
	                Brand       = (string)reader["brand"],
	                Year        = (int)reader["year"],
	                Color       = (string)reader["color"]
	            };
	        }
	    }


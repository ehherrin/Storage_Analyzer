using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Storage_Analyzer
{
	class Program
	{
		int dirCount;
		int fileCount;
		long length;

		public bool assess_command_validity(string flag, string path)
		{
			bool isValid =
					flag[0].Equals('-')
					& (
						flag[1].Equals('s')
						| flag[1].Equals('p')
						| flag[1].Equals('b')
					   )
					& flag.Length == 2
					& Directory.Exists(path);
			return isValid;
		}

		public void error_statement()
		{
			System.Console.WriteLine("Usage : Storage_Analyzer [-s] [-p] [-b] <path>");
			System.Console.WriteLine("Summarize disk usage of the set of FILES, recursively for directories .");
			System.Console.WriteLine("");
			System.Console.WriteLine("You MUST specify one of the parameters, -s, -p, or -b");
			System.Console.WriteLine("-s\tRun in single threaded mode");
			System.Console.WriteLine("-p\tRun in parallel mode (uses allavailable processors");
			System.Console.WriteLine("-b\tRun in both parallel and single threaded mode.");
			System.Console.WriteLine("  \tRuns parallel followed by sequential mode");
		}

		private void seqDirSearch(string sDir)
		{
			try
			{
				foreach (string f in Directory.GetFiles(sDir))
				{
					this.fileCount++;
					this.length += new System.IO.FileInfo(f).Length;
				}

				foreach (string d in Directory.GetDirectories(sDir))
				{
					this.dirCount++;
					this.seqDirSearch(d);
				}
			}
			catch (System.Exception)
			{
			}
		}

		private void parDirSearch(string sDir)
		{
			try
			{
				Parallel.ForEach(Directory.GetFiles(sDir), (f) =>
				{
					lock (this)
					{
						this.fileCount++;
						this.length += new System.IO.FileInfo(f).Length;
					}
				});
				Parallel.ForEach(Directory.GetDirectories(sDir), (d) =>
				{
					lock (this)
					{
						this.dirCount++;
					}
					this.parDirSearch(d);
				});
			}
			catch (System.Exception)
			{
			}
		}

		static void Main(string[] args)
		{
			Program n = new Program();
			try
			{
				if (n.assess_command_validity(args[0], args[1]))
				{
					System.Console.WriteLine();
					System.Console.WriteLine("Directory \'" + args[1] + "\' :");
					System.Console.WriteLine();
					Stopwatch parTime;
					Stopwatch seqTime;
					switch (args[0][1])
					{
						case 'p':
							parTime = Stopwatch.StartNew();
							n.parDirSearch(args[1]);
							parTime.Stop();
							Console.WriteLine("Parallel Calculated in: {0:0.0000000}s", parTime.Elapsed.TotalSeconds);
							System.Console.WriteLine(n.dirCount.ToString("###,###")
														+ " folders, "
														+ n.fileCount.ToString("###,###")
														+ " files, "
														+ n.length.ToString("###,###")
														+ " bytes");
							break;
						case 's':
							seqTime = Stopwatch.StartNew();
							n.seqDirSearch(args[1]);
							seqTime.Stop();
							Console.WriteLine("Sequential Calculated in: {0:0.0000000}s", seqTime.Elapsed.TotalSeconds);
							System.Console.WriteLine(n.dirCount.ToString("###,###")
														+ " folders, "
														+ n.fileCount.ToString("###,###")
														+ " files, "
														+ n.length.ToString("###,###")
														+ " bytes");
							break;
						case 'b':
							parTime = Stopwatch.StartNew();
							n.parDirSearch(args[1]);
							parTime.Stop();
							Console.WriteLine("Parallel Calculated in: {0:0.0000000}s", parTime.Elapsed.TotalSeconds);
							System.Console.WriteLine(n.dirCount.ToString("###,###")
														+ " folders, "
														+ n.fileCount.ToString("###,###")
														+ " files, "
														+ n.length.ToString("###,###")
														+ " bytes");
							System.Console.WriteLine();
							n.dirCount = 0;
							n.fileCount = 0;
							n.length = 0;
							seqTime = Stopwatch.StartNew();
							n.seqDirSearch(args[1]);
							seqTime.Stop();
							Console.WriteLine("Sequential Calculated in: {0:0.0000000}s", seqTime.Elapsed.TotalSeconds);
							System.Console.WriteLine(n.dirCount.ToString("###,###")
														+ " folders, "
														+ n.fileCount.ToString("###,###")
														+ " files, "
														+ n.length.ToString("###,###")
														+ " bytes");
							break;
						default:
							n.error_statement();
							break;
					}
				}
				else
				{
					n.error_statement();
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				n.error_statement();
			}
		}
	}
}
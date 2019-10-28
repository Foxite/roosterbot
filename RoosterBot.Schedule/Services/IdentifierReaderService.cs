namespace RoosterBot.Schedule {
	public class IdentifierReaderService {
		private MultiReader m_MultiReader;

		internal IdentifierReaderService(MultiReader mr) {
			m_MultiReader = mr;
		}

		public void AddReader<T>(IdentifierInfoReaderBase<T> reader) where T : IdentifierInfo {
			m_MultiReader.AddReader(reader);
		}
	}
}

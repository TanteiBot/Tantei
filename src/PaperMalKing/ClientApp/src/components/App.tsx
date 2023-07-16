import Footer from "./Footer";
import Header from "./Header";

function App() {
  return (
      <>
          <Header />
          <div className="container py-4 px-3 mx-auto">
              <h1>Hello, Bootstrap and Vite!</h1>
              <button className="btn btn-primary">Primary button</button>
          </div>
          <Footer />
      </>)
}

export default App

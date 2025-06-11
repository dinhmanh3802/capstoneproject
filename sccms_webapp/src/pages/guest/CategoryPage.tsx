import { MainSidePostList } from "../../components/Page/guest"
import CategoryPostList from "../../components/Page/guest/category/CategoryPostList"

const CategoryPage = () => {
    return (
        <div className="container-fluid">
            <div className="container">
                <div className="row">
                    <div className="col-lg-8">
                        <CategoryPostList />
                    </div>
                    <div className="col-lg-4" style={{ marginTop: "20px" }}>
                        <MainSidePostList />
                    </div>
                </div>
            </div>
        </div>
    )
}
export default CategoryPage
